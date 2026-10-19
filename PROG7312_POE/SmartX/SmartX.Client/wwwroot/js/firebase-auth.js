import { initializeApp } from "https://www.gstatic.com/firebasejs/13.0.0/firebase-app.js";

import {
    getAuth,
    createUserWithEmailAndPassword,
    signInWithEmailAndPassword,
    signOut as firebaseSignOut,
    onAuthStateChanged
} from "https://www.gstatic.com/firebasejs/13.0.0/firebase-auth.js";

// Firebase configuration for Smart-X IoT Management.
const firebaseConfig = {
    apiKey: "AIzaSyBCIqw_fPTcaZYRxuD5zf5kQA3SHLIllN0",
    authDomain: "smart-x-iot-management.firebaseapp.com",
    projectId: "smart-x-iot-management",
    storageBucket: "smart-x-iot-management.firebasestorage.app",
    messagingSenderId: "341387548517",
    appId: "1:341387548517:web:ff37d7c5204e06855a338c"
};

// Initialise Firebase and Authentication.
const app = initializeApp(firebaseConfig);
const auth = getAuth(app);

// Convert Firebase user information into a simple object for Blazor.
function toUserInfo(user) {
    if (!user) {
        return null;
    }

    return {
        uid: user.uid,
        email: user.email ?? ""
    };
}

// Register a new account.
export async function register(email, password) {
    const result = await createUserWithEmailAndPassword(
        auth,
        email,
        password
    );

    return toUserInfo(result.user);
}

// Sign in to an existing account.
export async function signIn(email, password) {
    const result = await signInWithEmailAndPassword(
        auth,
        email,
        password
    );

    return toUserInfo(result.user);
}

// Sign out of the current account.
export async function signOut() {
    await firebaseSignOut(auth);
}

// Wait for Firebase to determine the current authentication state.
function waitForUser() {
    return new Promise((resolve, reject) => {
        let unsubscribe = () => { };

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
    });
}

// Return the current signed-in user's information.
export async function getCurrentUser() {
    const user = await waitForUser();
    return toUserInfo(user);
}

// Retrieve an ID token for future API authentication.
export async function getIdToken() {
    const user = await waitForUser();

    if (!user) {
        return null;
    }

    return await user.getIdToken();
}
