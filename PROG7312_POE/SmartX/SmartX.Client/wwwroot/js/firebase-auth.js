
import { initializeApp } from "https://www.gstatic.com/firebasejs/13.0.0/firebase-app.js";

import {
    getAuth,
    createUserWithEmailAndPassword,
    signInWithEmailAndPassword,
    signOut as firebaseSignOut,
    onAuthStateChanged
} from "https://www.gstatic.com/firebasejs/13.0.0/firebase-auth.js";

// Firebase configuration for the Smart-X IoT Management project.
const firebaseConfig = {
    apiKey: "AIzaSyBCIqw_fPTcaZYRxuD5zf5kQA3SHLIl1N0",
    authDomain: "smart-x-iot-management.firebaseapp.com",
    projectId: "smart-x-iot-management",
    storageBucket: "smart-x-iot-management.firebasestorage.app",
    messagingSenderId: "341387548517",
    appId: "1:341387548517:web:ff37d7c5204e06855a338c"
};

// Initialise Firebase and its Authentication service.
const app = initializeApp(firebaseConfig);
const auth = getAuth(app);

// Return basic user information that Blazor can understand.
function toUserInfo(user) {
    if (!user) {
        return null;
    }

    return {
        uid: user.uid,
        email: user.email ?? ""
    };
}

// Register a new user using an email address and password.
export async function register(email, password) {
    const result = await createUserWithEmailAndPassword(
        auth,
        email,
        password
    );

    return toUserInfo(result.user);
}

// Sign in an existing registered user.
export async function signIn(email, password) {
    const result = await signInWithEmailAndPassword(
        auth,
        email,
        password
    );

    return toUserInfo(result.user);
}

// Sign out the currently signed-in user.
export async function signOut() {
    await firebaseSignOut(auth);
}

// Wait until Firebase has checked the current authentication state.
function waitForUser() {
    return new Promise((resolve, reject) => {
        let unsubscribe = () => { };

        try {
            unsubscribe = onAuthStateChanged(
                auth,
                (user) => {
                    unsubscribe();
                    resolve(user);
                },
                (error) => {
                    unsubscribe();
                    reject(error);
                }
            );
        } catch (error) {
            reject(error);
        }
    });
}

// Get information about the currently signed-in user.
export async function getCurrentUser() {
    const user = await waitForUser();
    return toUserInfo(user);
}

// Get a Firebase ID token for authenticating requests to the API.
export async function getIdToken() {
    const user = await waitForUser();

    if (!user) {
        return null;
    }

    return await user.getIdToken();
}
