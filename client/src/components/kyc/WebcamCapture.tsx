'use client';

import React, { useRef, useState, useEffect, useCallback } from 'react';
import Webcam from 'react-webcam';
import * as faceapi from 'face-api.js';
import { Camera, CheckCircle2, RotateCcw } from 'lucide-react';

interface WebcamCaptureProps {
  onCapture: (file: File) => void;
}

export default function WebcamCapture({ onCapture }: WebcamCaptureProps) {
  const webcamRef = useRef<Webcam>(null);
  const canvasRef = useRef<HTMLCanvasElement>(null);
  
  const [isModelsLoaded, setIsModelsLoaded] = useState(false);
  const [isDetecting, setIsDetecting] = useState(false);
  const [livenessStatus, setLivenessStatus] = useState({
    turnedLeft: false,
    turnedRight: false,
    isCenter: false,
  });
  const [photo, setPhoto] = useState<string | null>(null);

  useEffect(() => {
    const loadModels = async () => {
      try {
        const MODEL_URL = '/models';
        await Promise.all([
          faceapi.nets.tinyFaceDetector.loadFromUri(MODEL_URL),
          faceapi.nets.faceLandmark68Net.loadFromUri(MODEL_URL),
        ]);
        setIsModelsLoaded(true);
      } catch (err) {
        console.error("Failed to load models. Make sure they are in public/models", err);
      }
    };
    loadModels();
  }, []);

  const handleVideoOnLoad = () => {
    setIsDetecting(true);
  };

  useEffect(() => {
    let interval: any;
    if (isDetecting && isModelsLoaded && !photo) {
      interval = setInterval(async () => {
        if (webcamRef.current && webcamRef.current.video && webcamRef.current.video.readyState === 4) {
          const video = webcamRef.current.video;
          const displaySize = { width: video.videoWidth, height: video.videoHeight };
          
          if (canvasRef.current) {
             faceapi.matchDimensions(canvasRef.current, displaySize);
          }

          const detections = await faceapi.detectSingleFace(video, new faceapi.TinyFaceDetectorOptions()).withFaceLandmarks();
          
          if (detections) {
            if (canvasRef.current) {
                const resizedDetections = faceapi.resizeResults(detections, displaySize);
                const ctx = canvasRef.current.getContext('2d');
                ctx?.clearRect(0, 0, displaySize.width, displaySize.height);
                // Draw face oval (custom) or landmarks
                faceapi.draw.drawFaceLandmarks(canvasRef.current, resizedDetections);
            }

            const landmarks = detections.landmarks;
            const nose = landmarks.getNose()[0];
            const leftEye = landmarks.getLeftEye()[0];
            const rightEye = landmarks.getRightEye()[3]; // Outer edge

            const distLeft = nose.x - leftEye.x;
            const distRight = rightEye.x - nose.x;
            const ratio = distLeft / (distRight || 1);

            setLivenessStatus(prev => {
              const nextStatus = { ...prev };
              
              // Step 1: Must turn left first
              if (!prev.turnedLeft && ratio > 1.8) {
                nextStatus.turnedLeft = true;
              }
              // Step 2: Only after left is done, turn right
              else if (prev.turnedLeft && !prev.turnedRight && ratio < 0.55) {
                nextStatus.turnedRight = true;
              }
              
              // isCenter should always reflect the *current* state, not cached
              nextStatus.isCenter = (ratio >= 0.8 && ratio <= 1.2); 
              return nextStatus;
            });
          }
        }
      }, 300); // Check every 300ms
    }

    return () => clearInterval(interval);
  }, [isDetecting, isModelsLoaded, photo]);

  // Auto capture when liveness is verified
  useEffect(() => {
    // Only capture when they have completed both turns, and are CURRENTLY looking center
    if (livenessStatus.turnedLeft && livenessStatus.turnedRight && livenessStatus.isCenter && !photo) {
      // Add a tiny delay to let them stabilize face
      setTimeout(() => {
        capture();
      }, 500);
    }
  }, [livenessStatus, photo]);

  const capture = useCallback(() => {
    const imageSrc = webcamRef.current?.getScreenshot();
    if (imageSrc) {
      setPhoto(imageSrc);
      fetch(imageSrc)
        .then(res => res.blob())
        .then(blob => {
          const file = new File([blob], "face_capture.jpg", { type: "image/jpeg" });
          onCapture(file);
        });
    }
  }, [webcamRef, onCapture]);

  const retake = () => {
    setPhoto(null);
    setLivenessStatus({ turnedLeft: false, turnedRight: false, isCenter: false });
  };

  // Determine which step is currently active for UX
  const currentStep = !livenessStatus.turnedLeft ? 1 : (!livenessStatus.turnedRight ? 2 : 3);

  return (
    <div className="flex flex-col items-center">
      {!isModelsLoaded ? (
        <div className="flex flex-col items-center justify-center p-12 bg-slate-100 rounded-xl w-full max-w-md aspect-video border-2 border-dashed border-slate-300">
          <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-indigo-600 mb-4"></div>
          <p className="text-slate-600 font-medium">Loading AI Models...</p>
        </div>
      ) : (
        <div className="relative rounded-xl overflow-hidden shadow-lg border-4 border-indigo-100 bg-black max-w-md w-full">
          {photo ? (
            <img src={photo} alt="Captured face" className="w-full h-auto object-cover" />
          ) : (
             <div className="relative">
                <Webcam
                    audio={false}
                    ref={webcamRef}
                    screenshotFormat="image/jpeg"
                    videoConstraints={{ width: 640, height: 480, facingMode: "user" }}
                    onUserMedia={handleVideoOnLoad}
                    className="w-full h-auto object-cover"
                />
                <canvas
                    ref={canvasRef}
                    className="absolute top-0 left-0 w-full h-full pointer-events-none"
                />
                
                {/* Oval mask guide */}
                <div className="absolute inset-0 pointer-events-none border-[40px] border-black/50 rounded-[40%] shadow-[inset_0_0_20px_rgba(0,0,0,0.5)] flex items-center justify-center">
                   <div className="w-full h-full border-2 border-indigo-500/50 rounded-[inherit]"></div>
                </div>
             </div>
          )}

          {/* Liveness indicators Overlay */}
          {!photo && (
            <div className="absolute top-4 left-0 w-full px-4 flex flex-col gap-3">
                <div className="bg-black/60 shadow-lg text-white font-bold text-center py-2 rounded-xl backdrop-blur-md border border-white/20 animate-pulse">
                    {currentStep === 1 && "Bước 1: Vui lòng quay mặt TỪ TỪ sang TRÁI"}
                    {currentStep === 2 && "Bước 2: Tuyệt! Giờ hãy quay mặt sang PHẢI"}
                    {currentStep === 3 && "Bước 3: Nhìn thẳng vào CAMERA và giữ yên"}
                </div>
                
                <div className="flex justify-between gap-2">
                    <StatusBadge 
                        label="Sang Trái" 
                        active={livenessStatus.turnedLeft} 
                        isCurrent={currentStep === 1}
                        instruction="Đang chờ..." 
                    />
                    <StatusBadge 
                        label="Sang Phải" 
                        active={livenessStatus.turnedRight} 
                        isCurrent={currentStep === 2}
                        instruction="Đang chờ..." 
                    />
                    <StatusBadge 
                        label="Nhìn Thẳng" 
                        active={livenessStatus.turnedLeft && livenessStatus.turnedRight && livenessStatus.isCenter} 
                        isCurrent={currentStep === 3}
                        instruction="Giữ yên..." 
                    />
                </div>
            </div>
          )}
        </div>
      )}

      {photo && (
        <div className="mt-6 flex gap-4">
          <button
            onClick={retake}
            className="flex items-center gap-2 px-4 py-2 border border-slate-300 rounded-lg text-slate-700 hover:bg-slate-50 transition-colors"
          >
            <RotateCcw className="w-4 h-4" />
            Retake Photo
          </button>
          <div className="flex items-center gap-2 px-4 py-2 bg-green-50 text-green-700 border border-green-200 rounded-lg font-medium">
             <CheckCircle2 className="w-4 h-4" />
             Face Verified & Captured
          </div>
        </div>
      )}
    </div>
  );
}

function StatusBadge({ label, active, isCurrent, instruction }: { label: string, active: boolean, isCurrent?: boolean, instruction: string }) {
    return (
        <div className={`flex flex-col items-center p-2 rounded-lg backdrop-blur-md border ${
            active ? 'bg-green-500/80 border-green-400 text-white shadow-[0_0_15px_rgba(34,197,94,0.5)]' 
                   : (isCurrent ? 'bg-indigo-600/80 border-indigo-400 text-white shadow-[0_0_15px_rgba(79,70,229,0.5)] scale-105' : 'bg-black/40 border-white/20 text-white/70')
        } transition-all duration-300 flex-1`}>
            <span className="text-xs font-bold uppercase tracking-wider">{label}</span>
            <span className="text-[10px] mt-1 opacity-80 text-center">{active ? 'Hoàn thành' : instruction}</span>
        </div>
    )
}
