from fastapi import FastAPI, HTTPException
from fastapi.responses import FileResponse
from fastapi.background import BackgroundTasks
from pydantic import BaseModel
import subprocess
import os
import uuid
import tempfile

app = FastAPI(title="Piper TTS API", description="Simple API for Text-to-Speech using Piper")

class TTSRequest(BaseModel):
    text: str

def remove_file(path: str):
    """Remove a file if it exists"""
    if os.path.exists(path):
        os.unlink(path)

@app.post("/tts", summary="Convert text to speech")
async def text_to_speech(request: TTSRequest, background_tasks: BackgroundTasks):
    """
    Convert the provided text to speech using Piper.
    
    Returns a WAV file of the synthesized speech.
    """
    if not request.text:
        raise HTTPException(status_code=400, detail="No text provided")
    
    # Create a unique temporary file
    temp_dir = tempfile.gettempdir()
    unique_id = uuid.uuid4()
    temp_wav = os.path.join(temp_dir, f"tts_output_{unique_id}.wav")
    
    try:
        # Call piper.exe to generate speech
        process = subprocess.Popen(
            ["piper/piper.exe", "-m", "piper/Thorsten-Voice_Hessisch_Piper_high-Oct2023.onnx", "-f", temp_wav],
            stdin=subprocess.PIPE, 
            stdout=subprocess.PIPE, 
            stderr=subprocess.PIPE,
            text=True
        )
        
        # Send text to piper
        stdout, stderr = process.communicate(input=request.text)
        
        if process.returncode != 0:
            raise HTTPException(status_code=500, detail=f"Piper error: {stderr}")
        
        if not os.path.exists(temp_wav):
            raise HTTPException(status_code=500, detail="Failed to generate audio file")
        
        # Schedule file deletion after response is sent
        background_tasks.add_task(remove_file, temp_wav)
        
        # Return the WAV file
        return FileResponse(
            temp_wav, 
            media_type="audio/wav", 
            filename="tts_output.wav"
        )
    
    except Exception as e:
        # Clean up in case of error
        if os.path.exists(temp_wav):
            os.unlink(temp_wav)
        raise HTTPException(status_code=500, detail=f"Error processing request: {str(e)}")

@app.get("/", summary="API Health Check")
async def read_root():
    """Health check endpoint"""
    return {"status": "ok", "message": "Piper TTS API is running"}

