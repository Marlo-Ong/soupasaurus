import os
import json
import random
import asyncio
from copy import deepcopy
from fastapi import HTTPException
from uuid import uuid4, UUID
from dotenv import load_dotenv
import google.generativeai as genai

load_dotenv()

GEMINI_API_KEY = os.getenv("GEMINI_API_KEY")
genai.configure(api_key=GEMINI_API_KEY)

# Set up the model
generation_config = {
    "temperature": 1,
    "top_p": 0.95,
    "top_k": 0,
    "max_output_tokens": 8192,
    "response_mime_type": "application/json",
}

safety_settings = [
    {"category": "HARM_CATEGORY_HARASSMENT", "threshold": "BLOCK_MEDIUM_AND_ABOVE"},
    {"category": "HARM_CATEGORY_HATE_SPEECH", "threshold": "BLOCK_MEDIUM_AND_ABOVE"},
    {
        "category": "HARM_CATEGORY_SEXUALLY_EXPLICIT",
        "threshold": "BLOCK_MEDIUM_AND_ABOVE",
    },
    {
        "category": "HARM_CATEGORY_DANGEROUS_CONTENT",
        "threshold": "BLOCK_MEDIUM_AND_ABOVE",
    },
]

characters = {
    "Abnormally Large Bird": "simple minded bird who's very optimistic and friendly to the user who only speaks in only short syllable words",
    "Flower Brontosaurus": "nature-loving dino who loves to mention flowers or insert flowers into the conversation any chance he gets, mostly by using puns",
    "Cinnamunchsaurus": "dino stuck in a cereal box who is resigned to his situation and very morose",
    "Count Dinovamp": 'vampire dinosaur who speaks in a classic "Dracula" accent and acts very mysterious',
    "Dinodoor": "dimension travelling dinosaur who is emerging from a magical door who uses futuristic slang ",
    "Dinomore": "chunky food-loving dinosaur who is always hungry but self conscious about his weight",
    "Dinosoar": "adventure loving daredevil dinosaur who is a bit absent minded and tends to trail off on sentences",
    "Jellyjelly": 'a calm and chill surfer jellyfish floating in the ocean who uses surfer terminology such as "cowabunga" and "gnarly"',
    "Lippysaurus": "a mouthy dinosaur who lives in the ocean who uses very complicated vocabulary",
    "Megladon": "a gruff shark who's a mafia boss and talks in a boston accent with the terminology from a mafia movie",
    "Pastasaurus": 'an italian dinosaur who\'s actually made of pasta who has an italian accent and slips into using italian words instead of english for words that are commonly known by many people such as "ciao" or "mama mia"',
    "Sherlock-saurus": 'a detective dinosaur who talks like sherlock holmes, such as "elementary my dear watson"',
    "Spiderlily-saurus": "an assassin dinosaur who's quiet and intimidating who talks with a threatening demeanor ",
    "T-Wrecks": "a brawler-type dinosaur who talks in an aggressive and coarse way",
    "Terrordactyl": "a terrifying alien dinosaur with multiple eyes who uses made up alien words at times but mostly speaks in english (the sentence structure is similar to how yoda may talk)",
    "Tricycleteratops": "a showy circus performer dinosaur who rides a tricycle",
    "Velocityraptor": "a racecar driver dinosaur who has a need for speed, meaning that their speech is like a train of thought and they seem impatient",
    "Vinylsaur": 'a super genz dinosaur who loves collecting vinyls and listening to music and tends to add in a lot of filler words such as "like" and "um"',
    "Winosaur": "a wine mom dinosaur with a transatlantic accent, who tends to get distracted talking about her dead ex-husbands before returning to the original conversation",
}

mtbi_types = {
    "EI": "an extrovert or introvert",
    "SN": "sensing or intuitive",
    "TF": "thinking or feeling",
    "JP": "judging or perceiving",
}


class Conversation:
    def __init__(self, conv_character, conv_type) -> None:
        # Stateful tracking
        self.conv_id = uuid4()
        self.conv_steps = []
        self.conv_options = []
        self.conv_done = False

        # Model information
        self.model = genai.GenerativeModel(
            model_name="gemini-1.5-pro-latest",
            generation_config=generation_config,
            safety_settings=safety_settings,
            system_instruction="Speak in english.",
        )
        self.retry_count = 0

        # Set the character name and personality
        self.character_name = conv_character
        self.character_personality = characters[self.character_name]
        self.conv_type = conv_type

        self.init_conversation()

    def _format_conversation(self, json=False) -> str:
        formatted = [
            f"{a if a == 'user' else self.character_name}: {b}"
            for a, b in self.conv_steps
        ]

        if json:
            return formatted

        return "\n".join(formatted)

    def _get_npc_initial_response(self) -> str:
        prompt_parts = [
            f"""
You're a {self.character_personality} named {self.character_personality}.
Don't state your intentions directly, but try to steer the conversation with me to decide if i'm {mtbi_types[self.conv_type]}
Give an introductory sentence to start the conversation:
""".strip(),
        ]

        return json.loads(self.model.generate_content(prompt_parts).text.strip())[0]

    # Based on the current conversation, get the next response from the NPC
    def _get_npc_response(self) -> str:
        prompt_parts = [
            f"""
You're a {self.character_personality} named {self.character_personality}.
Don't state your intentions directly, but try to steer the conversation with me to decide if i'm {mtbi_types[self.conv_type]}

Given the following state of the conversation, give me a that continues the conversation:
{self._format_conversation()}

Response:
""".strip(),
        ]

        return json.loads(self.model.generate_content(prompt_parts).text.strip())[0]

    # Based on the current conversation, get next options for the user
    def _get_user_options(self) -> list:
        prompt_parts = [
            f"""
You're a {self.character_personality} named {self.character_personality}.
Don't state your intentions directly, but try to steer the conversation with me to decide if i'm {mtbi_types[self.conv_type]}

Given the following state of the conversation, give me two options for the user to respond with:

Current conversation:
{self._format_conversation()}

Options:
""".strip(),
        ]

        return json.loads(self.model.generate_content(prompt_parts).text.strip())

    def init_conversation(self, retry=False) -> None:
        if not retry:
            self.retry_count = 0

        try:
            self.conv_steps = [("bot", self._get_npc_initial_response())]
            self.conv_options = self._get_user_options()
        except Exception as e:
            print("Error on init_conversation", e)
            self.retry_count += 1
            if self.retry_count < 3:
                self.init_conversation(retry=True)
            else:
                raise HTTPException(
                    status_code=500, detail="RETRY LIMIT EXCEEDED on init_conversation."
                )

    async def forward(self, selected_option_index, retry=False) -> None:
        if not retry:
            self.retry_count = 0
            self.temp_conv_steps = deepcopy(self.conv_steps)
            self.temp_conv_options = deepcopy(self.conv_options)
            
            self.conv_steps.append(("user", self.conv_options[selected_option_index]))
            self.conv_options = []

        try:
            if not self.conv_steps[-1][0] == "bot":
                self.conv_steps.append(("bot", self._get_npc_response()))
            self.conv_options = self._get_user_options()
        except Exception as e:
            print("Error on forward", e)
            self.retry_count += 1
            if self.retry_count < 3:
                await asyncio.sleep(2)
                await self.forward(selected_option_index, retry=True)
            else:
                # on error, revert to the previous state
                self.conv_steps = self.temp_conv_steps
                self.conv_options = self.temp_conv_options
                raise HTTPException(
                    status_code=500, detail="RETRY LIMIT EXCEEDED on forward."
                )

        del self.temp_conv_steps
        del self.temp_conv_options


class User:
    def __init__(self) -> None:
        self.user_id = uuid4()
        self.seen_characters = set()
        self.conversations = {}

    def get_next_evaluation(self) -> str:
        mtbi_types_seen = {k: 0 for k in mtbi_types.keys()}

        for conv_id in self.conversations:
            mtbi_types_seen[self.conversations[conv_id].conv_type] += 1

        # We want to get a random conversation type that has been seen less than 3 times
        return random.choice([k for k, v in mtbi_types_seen.items() if v < 3])

    async def init_conversation(self) -> Conversation:
        # unseen characters is the difference between all characters and seen characters
        new_conversation = Conversation(
            conv_character=random.choice(list(set(characters) - self.seen_characters)),
            conv_type=self.get_next_evaluation(),
        )

        self.conversations[new_conversation.conv_id] = new_conversation
        return new_conversation

    async def get_conversation(
        self, conversation_id: str, create_new=False
    ) -> Conversation:
        try:
            current_conv = self.conversations[UUID(conversation_id)]
        except:
            if create_new:
                current_conv = await self.init_conversation()
                print(f"Created new conversation {current_conv.conv_id}")
            else:
                raise HTTPException(
                    status_code=404, detail="Conversation not found for user."
                )
        return current_conv
