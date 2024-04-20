import uvicorn
import typing
from uuid import UUID
from gemini import User
from fastapi import FastAPI, HTTPException

app = FastAPI()

# users is {uuid: User}
app.users = {}


@app.get("/new_user_id")
async def new_user_id():
    current_user = User()
    app.users[current_user.user_id] = current_user
    print(f"Created new user {current_user.user_id}")
    return {"user_id": str(current_user.user_id)}


@app.get("/new_options")
async def new_options(user_id: str, conversation_id: str):
    current_user = app.users[UUID(user_id)]
    current_conv = current_user.get_conversation(conversation_id)
    current_conv.forward()
    return {
        "user_id": user_id,
        "conversation_id": current_conv.conv_id,
        "options": current_conv.conv_options,
    }


@app.post("/conversation")
async def post_conversation(
    user_id: str,
    conversation_id: typing.Optional[str] = None,
    selected_option_index: typing.Optional[int] = None,
):
    current_user: User = app.users[UUID(user_id)]

    if selected_option_index is not None and conversation_id is None:
        return HTTPException(
            status_code=400, detail="Need conversation_id to select option."
        )

    current_conv = current_user.get_or_create_new_conversation(conversation_id)

    if current_conv.done:
        return HTTPException(status_code=400, detail="Conversation is done.")

    if selected_option_index is not None:
        current_conv.forward(selected_option_index)

    return {
        "user_id": user_id,
        "conversation_id": current_conv.conv_id,
        "character_name": current_conv.character_name,
        "response": current_conv.conv_steps[-1][1],
        "options": current_conv.conv_options,
        "done": current_conv.done,
    }


@app.get("/conversation/{user_id}/{conversation_id}")
async def get_conversation(user_id: str, conversation_id: str):
    current_user = app.users[UUID(user_id)]
    current_conv = current_user.get_conversation(conversation_id)
    return {
        "user_id": user_id,
        "conversation_id": current_conv.conv_id,
        "character_name": current_conv.character_name,
        "history": current_conv.__format_conversation(),
        "options": current_conv.conv_options,
        "done": current_conv.done,
    }


if __name__ == "__main__":
    uvicorn.run(app, host="0.0.0.0", port=10000)
