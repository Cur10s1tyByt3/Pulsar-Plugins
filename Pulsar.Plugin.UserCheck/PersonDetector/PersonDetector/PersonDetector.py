import argparse
import cv2
from deepface import DeepFace

def main():
    parser = argparse.ArgumentParser(description="Detect and analyze a person in an image.")
    parser.add_argument("--image", required=False, help="Path to the image file.")
    args = parser.parse_args()
    
    if not args.image:
        image_path = input("Enter the path to the image file: ")
    else:
        image_path = args.image
    
    print("[*] Analyzing image... This may take a few seconds.")
    try:
        result = DeepFace.analyze(img_path=image_path, actions=['age', 'gender', 'emotion', 'race'], enforce_detection=True)
    except Exception as e:
        print(f"[!] Error analyzing image: {e}")
        return
    
    if isinstance(result, list):
        result = result[0]
    
    dominant_emotion = result['dominant_emotion']
    age = result['age']
    gender = result['gender']
    dominant_race = result['dominant_race']
    
    print(f"[*] Analysis Complete:")
    print(f"    Age: {age}")
    print(f"    Gender: {gender}")
    print(f"    Dominant Emotion: {dominant_emotion}")
    print(f"    Dominant Race: {dominant_race}")
    
    image = cv2.imread(image_path)
    if image is None:
        print("[!] Failed to load image.")
        return
    
    if isinstance(gender, dict):
        dominant_gender = max(gender, key=gender.get)
    else:
        dominant_gender = gender
    
    text_age = f"Age: {age}"
    text_gender = f"Gender: {dominant_gender}"
    text_emotion = f"Emotion: {dominant_emotion}"
    text_race = f"Race: {dominant_race}"
    
    if "region" in result and result["region"]:
        region = result["region"]
        x = region.get('x', 0)
        y = region.get('y', 0)
        w = region.get('w', 0)
        h = region.get('h', 0)
        cv2.rectangle(image, (x, y), (x+w, y+h), (0,255,0), 2)
        # column
        cv2.putText(image, text_age, (x+w+10, y), cv2.FONT_HERSHEY_SIMPLEX, 0.7, (0,255,0), 2)
        cv2.putText(image, text_gender, (x+w+10, y+30), cv2.FONT_HERSHEY_SIMPLEX, 0.7, (0,255,0), 2)
        cv2.putText(image, text_emotion, (x+w+10, y+60), cv2.FONT_HERSHEY_SIMPLEX, 0.7, (0,255,0), 2)
        cv2.putText(image, text_race, (x+w+10, y+90), cv2.FONT_HERSHEY_SIMPLEX, 0.7, (0,255,0), 2)
    
    cv2.imshow("Person Detector", image)
    cv2.waitKey(0)
    cv2.destroyAllWindows()

if __name__ == "__main__":
    main()