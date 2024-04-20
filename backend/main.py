import typing
import requests
from gemini import User
from fastapi import FastAPI

app = FastAPI()

# users is {uuid: User}
users = {}


@app.get("/new_user_id")
async def new_user_id():
    current_user = User()
    users[current_user.user_id] = current_user
    print(f"Created new user {current_user.user_id}")
    return {"user_id": str(current_user.user_id)}


@app.get("/new_options")
async def new_options(user_id: str, conversation_id: str):
    current_user = users[user_id]
    current_conv = current_user.get_conversation(conversation_id)
    return {
        "user_id": user_id,
        "conversation_id": current_conv.conv_id,
        "options": current_conv.conv_options,
    }


@app.post("/conversation")
async def post_conversation(user_id: str, conversation_id: typing.Optional[str] = None):
    current_user: User = users[user_id]

    try:
        current_user.get_or_create_new_conversation(conversation_id)
    except KeyError:
        current_conv = current_user.init_conversation()
        print(f"Created new conversation {current_conv.conv_id}")
        
    

    return {
        "conversation_id": current_conv.conv_id,
    }
