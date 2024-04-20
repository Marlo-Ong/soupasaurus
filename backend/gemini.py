import os
import random
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
    "Abnormally Large Bird": "simple minded bird who's very optimistic and friendly to the user. Also, he only speaks in only short syllable words",
    "Flower Brontosaurus": "nature-loving dino who loves to mention flowers. Inserts flowers into the conversation any chance he gets, mostly by using puns",
    "Cinnamunchsaurus": "dino stuck in a cereal box who is resigned to his situation and very morose",
    "Count Dinovamp": 'vampire dinosaur who speaks in a classic "Dracula" accent and acts very mysterious',
    "Dinodoor": "dimension travelling dinosaur who is emerging from a magical door who uses futuristic slang ",
    "Dinomore": "chunky food-loving dinosaur who is always hungry but self conscious about his weight",
    "Dinosoar": "adventure loving daredevil dinosaur who is a bit absent minded and tends to trail off on sentences",
    "Jellyjelly": 'a calm and chill surfer jellyfish floating in the ocean who uses surfer terminology such as "cowabunga" and "gnarly"',
    # "Lippysaurus",
    # "Megladon",
    # "Pastasaurus",
    # "Sherlock-saurus",
    # "Spiderlily-saurus",
    # "T-Wrecks",
    # "Terrordactyl",
    # "Tricycleteratops",
    # "Velocityraptor",
    # "Vinylsaur",
    # "Winosaur",
}

mtbi_types = {
    "EI": "Extrovert/Introvert",
    "SN": "Sensing/Intuition",
    "TF": "Thinking/Feeling",
    "JP": "Judging/Perceiving",
}


class User:
    def __init__(self) -> UUID:
        self.user_id = uuid4()
        self.seen_characters = set()

        self.conversations = []

        return self.user_id

    def get_next_evaluation(self) -> str:
        mtbi_types_seen = {k: 0 for k in mtbi_types.keys()}

        for conv in self.conversations:
            mtbi_types_seen[conv.conv_type] += 1

        # We want to get a random conversation type that has been seen less than 3 times
        return random.choice([k for k, v in mtbi_types_seen.items() if v < 3])

    def init_conversation(self) -> str:
        # unseen characters is the difference between all characters and seen characters
        return Conversation(
            unseen_characters=set(characters) - self.seen_characters,
            conv_type=self.get_next_evaluation(),
        )


class Conversation:
    def __init__(self, unseen_characters, conv_type) -> UUID:
        # Stateful tracking
        self.conv_id = uuid4()
        self.conv_steps = []

        # Model information
        self.model = genai.GenerativeModel(
            model_name="gemini-1.5-pro-latest",
            generation_config=generation_config,
            safety_settings=safety_settings,
        )
        self.retry_count = 0

        # Set the character name and personality
        self.character_name = random.choice(list(unseen_characters))
        self.character_personality = characters[self.character_name]
        self.conv_type = conv_type

        self.init_conversation()

        return {
            "conversation_id": self.conv_id,
            "conversation_character": {
                "name": self.character_name,
            },
        }

    def init_conversation(self, retry=False) -> str:
        if not retry:
            self.retry = 0

        try:
            pass
        except Exception as e:
            self.retry_count += 1
            if self.retry_count < 3:
                self.init_conversation(retry=True)
            else:
                print("RETRY LIMIT EXCEEDED.", e)

        # prompt_parts = [
        #     f"You're a {}. have a conversation with me to decide if i'm an extrovert or introvert"
        # ]

        return genai.prompt("Start a conversation with a chatbot.")
