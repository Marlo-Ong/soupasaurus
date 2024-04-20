import typing
import requests
from gemini import Conversation
from fastapi import FastAPI

app = FastAPI()

# Conversations is {uuid: Conversation}
conversations = {}


@app.get("/conversation/{conversation_id}")
async def get_conversation(conversation_id):
    return {
        "conversation_id": conversation_id,
    }


@app.post("/conversation")
async def post_conversation(conversation_id: typing.Optional[str] = None):
    current_conv: Conversation = None
    if conversation_id is None or conversation_id not in conversations:
        current_conv = Conversation("EI", "alien")
        conversations[current_conv.conv_id] = current_conv

    return {
        "conversation_id": current_conv.conv_id,
    }
