conda env create
conda activate backend
pip install -r requirements.txt
uvicorn main:app --reload