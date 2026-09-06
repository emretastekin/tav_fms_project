// Import the functions you need from the SDKs you need
import { initializeApp } from "firebase/app";
import { getAuth } from "firebase/auth";
// TODO: Add SDKs for Firebase products that you want to use
// https://firebase.google.com/docs/web/setup#available-libraries

// Your web app's Firebase configuration
const firebaseConfig = {
  apiKey: "AIzaSyBH0eZSu-NCgjcVqAbrOSk0wvEIDzLgjjw",
  authDomain: "tavfms.firebaseapp.com",
  projectId: "tavfms",
  storageBucket: "tavfms.firebasestorage.app",
  messagingSenderId: "977672152433",
  appId: "1:977672152433:web:6935dac6ca18c434cf02d0"
};

// Initialize Firebase
const app = initializeApp(firebaseConfig);

// Kimlik doğrulama modülünü dışa aktar (Login ekranında kullanacağız)
export const auth = getAuth(app);
