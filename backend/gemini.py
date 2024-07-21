import time
import json
import random
import asyncio
from copy import deepcopy
from fastapi import HTTPException
from uuid import uuid4, UUID
import google.generativeai as genai

api_keys = json.load(open("api_keys.json"))

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

mtbi_split_types = {
    "EI": ["extroverted", "introverted"],
    "SN": ["sensing", "intuitive"],
    "TF": ["thinking", "feeling"],
    "JP": ["judging", "perceiving"],
}

mtbi_types_to_letters = {
    "extroverted": "E",
    "introverted": "I",
    "sensing": "S",
    "intuitive": "N",
    "thinking": "T",
    "feeling": "F",
    "judging": "J",
    "perceiving": "P",
}

mtbi_ingredients = {
    "EI": ["bold soup base", "light soup base"],
    "SN": ["typical vegetable", "exotic vegetable"],
    "TF": ["typical spice", "exotic spice"],
    "JP": ["red meat/fish", "white meat/white fish"],
}


class Conversation:
    def __init__(self, conv_character, conv_type) -> None:
        # Stateful tracking
        self.conv_id = uuid4()
        self.conv_steps = []
        self.conv_options = []
        self.conv_done = False

        # Conclusion info
        self.mtbi = None
        self.ingredient = None

        # Model information
        self.api_key_state = {
            key: {
                "uses_left": limit,
                "last_refreshed": time.time(),
            }
            for key, limit in api_keys.items()
        }
        self.__reconfigure_model()
        self.retry_count = 0

        # Set the character name and personality
        self.character_name = conv_character
        self.character_personality = characters[self.character_name]
        self.conv_type = conv_type

        self.init_conversation()

    def __reconfigure_model(self) -> None:
        # get a list of selectable keys, e.g. if they still have uses left or if 2 minutes have passed since last refresh
        selectable_keys = [
            key
            for key, value in self.api_key_state.items()
            if value["uses_left"] > 0
            or value["last_refreshed"] - time.time() > (60 * 2)
        ]

        # select and refresh if needed
        selected_key = random.choice(selectable_keys)
        self.api_key_state[selected_key]["uses_left"] -= 1
        if self.api_key_state[selected_key]["last_refreshed"] - time.time() > (60 * 2):
            self.api_key_state[selected_key] = {
                "uses_left": api_keys[selected_key],
                "last_refreshed": time.time(),
            }

        print(json.dumps(self.api_key_state, indent=2))

        genai.configure(api_key=selected_key)
        self.model = genai.GenerativeModel(
            model_name="gemini-1.5-pro-latest",
            generation_config=generation_config,
            safety_settings=safety_settings,
            system_instruction='Speak in english.\nOnly return responses in lists. For example, ["response"]. Do not include names or headers in the response.',
        )

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

        self.__reconfigure_model()
        return json.loads(self.model.generate_content(prompt_parts).text.strip())[0]

    # Based on the current conversation, get the next response from the NPC
    def _get_npc_response(self) -> str:
        prompt_parts = [
            f"""
You're a {self.character_personality} named {self.character_personality}.
Don't state your intentions directly, but try to steer the conversation with me to decide if i'm {mtbi_types[self.conv_type]}

Given the following state of the conversation, give me a response that continues the conversation.
{self._format_conversation()}

Response:
""".strip(),
        ]

        self.__reconfigure_model()
        return json.loads(self.model.generate_content(prompt_parts).text.strip())[0]

    def _get_npc_final_response(self) -> str:
        prompt_parts = [
            f"""
You're a {self.character_personality} named {self.character_personality}. You have been having a conversation with me to determine if i'm {mtbi_types[self.conv_type]}

Below is the conversation so far:
{self._format_conversation()}

End the conversation now and offer me an ingredient based on the character of the user that {self.character_name} has been talking to. Offer me {mtbi_ingredients[self.conv_type][0]} if i'm {mtbi_split_types[self.conv_type][0]}. If i'm {mtbi_split_types[self.conv_type][1]}, offer me a {mtbi_ingredients[self.conv_type][1]}.
Come up with the proper noun names of these ingredients.
Write a final message that flows well with the rest of the conversation.
Word it like the ingredient is being given to the user
Include the ingredient in the message
Ingredients should be normal, and fit into a soup recipe that you could cook at home
Ingredients should not be too exotic or out of place, but match the character, and the mbti_type selected
Ingredients should not be dinosaur or dragon related.

Use the following format and types:
{{
    "mtbi_type": "{mtbi_split_types[self.conv_type][0]}" | "{mtbi_split_types[self.conv_type][1]}"
    "ingredient": string
    "message": string
}}
""".strip(),
        ]

        self.__reconfigure_model()
        generation = json.loads(self.model.generate_content(prompt_parts).text.strip())
        if type(generation) == list:  # Sometimes the response is a list
            generation = generation[0]

        assert (
            "ingredient" in generation
        ), "ingredient not found in generation." + json.dumps(generation)
        assert (
            "mtbi_type" in generation
        ), "mtbi_type not found in generation." + json.dumps(generation)
        assert "message" in generation, "message not found in generation." + json.dumps(
            generation
        )

        return generation

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

        self.__reconfigure_model()
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

    async def set_new_user_options(self, retry=False) -> None:
        if not retry:
            self.retry_count = 0
            self.temp_conv_options = deepcopy(self.conv_options)

            self.conv_options = []

        try:
            self.conv_options = self._get_user_options()
        except Exception as e:
            print("Error on set_new_user_options", e)
            self.retry_count += 1
            if self.retry_count < 3:
                await asyncio.sleep(5)
                await self.set_new_user_options(retry=True)
            else:
                # on error, revert to the previous state
                self.conv_options = self.temp_conv_options
                raise HTTPException(
                    status_code=500,
                    detail="RETRY LIMIT EXCEEDED on set_new_user_options.",
                )

    async def forward(self, selected_option_index, retry=False) -> None:
        if not retry:
            self.retry_count = 0
            self.temp_conv_steps = deepcopy(self.conv_steps)
            self.temp_conv_options = deepcopy(self.conv_options)

            self.conv_steps.append(("user", self.conv_options[selected_option_index]))
            self.conv_options = []

            if len([step for step in self.conv_steps if step[0] == "user"]) >= 3:
                self.conv_done = True

        try:
            if not self.conv_steps[-1][0] == "bot":
                if not self.conv_done:
                    self.conv_steps.append(("bot", self._get_npc_response()))
                else:
                    final_assessment = self._get_npc_final_response()
                    self.mtbi = final_assessment["mtbi_type"]
                    self.ingredient = final_assessment["ingredient"]
                    self.conv_steps.append(("bot", final_assessment["message"]))
            if not self.conv_done:
                self.conv_options = self._get_user_options()
        except Exception as e:
            print("Error on forward", e)
            self.retry_count += 1
            if self.retry_count < 3:
                await asyncio.sleep(5)
                await self.forward(selected_option_index, retry=True)
            else:
                # on error, revert to the previous state
                self.conv_steps = self.temp_conv_steps
                self.conv_options = self.temp_conv_options
                self.conv_done = False
                raise HTTPException(
                    status_code=500, detail="RETRY LIMIT EXCEEDED on forward."
                )


class User:
    def __init__(self) -> None:
        self.user_id = uuid4()
        self.seen_characters = set()
        self.conversations = {}

        self.overall_mtbi = ""
        self.soup_name = None
        self.soup_ingredients = []

        # Model information
        self.api_key_state = {
            key: {
                "uses_left": limit,
                "last_refreshed": time.time(),
            }
            for key, limit in api_keys.items()
        }
        self.__reconfigure_model()

    def __reconfigure_model(self) -> None:
        # get a list of selectable keys, e.g. if they still have uses left or if 2 minutes have passed since last refresh
        selectable_keys = [
            key
            for key, value in self.api_key_state.items()
            if value["uses_left"] > 0
            or value["last_refreshed"] - time.time() > (60 * 2)
        ]

        # select and refresh if needed
        selected_key = random.choice(selectable_keys)
        self.api_key_state[selected_key]["uses_left"] -= 1
        if self.api_key_state[selected_key]["last_refreshed"] - time.time() > (60 * 2):
            self.api_key_state[selected_key] = {
                "uses_left": api_keys[selected_key],
                "last_refreshed": time.time(),
            }

        print(json.dumps(self.api_key_state, indent=2))

        genai.configure(api_key=selected_key)
        self.model = genai.GenerativeModel(
            model_name="gemini-1.5-pro-latest",
            generation_config=generation_config,
            safety_settings=safety_settings,
            system_instruction="Speak in english.\nDo not include names or headers in the response.",
        )

    def _get_soup_score_tm(self) -> str:
        prompt_parts = [
            f"""
Our brave user has been talking to a variety of characters to determine their personality types. The user has been talking to the following characters:
{", ".join(self.seen_characters)}

From these characters, the user has received a variety of ingredients:
{", ".join(self.soup_ingredients)}

The user has been defined as a {self.overall_mtbi} personality type.

Based on this information, name a soup type, (for example "Chicken Noodle Soup", or "Tomato bisque") based on the ingredients provided.
Select only up to six ingredients from the list of ingredients provided.
The soup should have a creative name, as if it were being presented in a michelin star restaurant.
As often as possible, most of the ingredients should be compatable with the chosen soup type.

Use the following format and types:
{{
    "used_ingredients": Array<string>
    "soup_type": string
}}
""".strip(),
        ]

        self.__reconfigure_model()
        generation = json.loads(self.model.generate_content(prompt_parts).text.strip())
        if type(generation) == list:  # Sometimes the response is a list
            generation = generation[0]

        assert (
            "soup_type" in generation
        ), "soup_type not found in generation." + json.dumps(generation)
        assert (
            "used_ingredients" in generation
        ), "used_ingredients not found in generation." + json.dumps(generation)

        return generation

    def get_next_evaluation(self) -> str:
        mtbi_types_seen = {k: 0 for k in mtbi_types.keys()}

        for conv_id in self.conversations:
            mtbi_types_seen[self.conversations[conv_id].conv_type] += 1

        # We want to get a random conversation type that has been seen less than 3 times
        return min([(k,v) for k, v in mtbi_types_seen.items() if v < 3], key=lambda x: x[1])[0]

    async def init_conversation(self) -> Conversation:
        # unseen characters is the difference between all characters and seen characters
        random_character = random.choice(list(set(characters) - self.seen_characters))
        seen_characters.add(random_character)

        new_conversation = Conversation(
            conv_character=random_character,
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

    async def set_user_soup(self) -> None:
        self.soup_ingredients = []
        self.overall_mtbi = ""
        self.soup_name = None

        overall_mtbi_scores = {k: 0 for k in mtbi_types.keys()}

        for conv_id in self.conversations:
            if not self.conversations[conv_id].conv_done:
                raise HTTPException(
                    status_code=400,
                    detail="Cannot start soup process without all conversations having been wrapped up.",
                )
            self.soup_ingredients.append(self.conversations[conv_id].ingredient)
            bias_letter = mtbi_types_to_letters[self.conversations[conv_id].mtbi]
            for score_region in overall_mtbi_scores:
                if bias_letter in score_region:
                    overall_mtbi_scores[score_region] += (
                        -1 if score_region.index(bias_letter) == 0 else 1
                    )
                    break

        for score_region in overall_mtbi_scores:
            if overall_mtbi_scores[score_region] > 0:
                self.overall_mtbi += score_region[0]
            elif overall_mtbi_scores[score_region] < 0:
                self.overall_mtbi += score_region[1]
            else:
                self.overall_mtbi += random.choice(score_region)

        da_soup_score = self._get_soup_score_tm()
        self.soup_name = da_soup_score["soup_type"]
        self.soup_ingredients = da_soup_score["used_ingredients"]
