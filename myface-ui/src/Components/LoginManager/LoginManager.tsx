import React, {createContext, ReactNode, useState} from "react";

export const LoginContext = createContext({
    isLoggedIn: false,
    isAdmin: false,
    logIn: (username: string, password: string) => {},
    logOut: () => {},
});

interface LoginManagerProps {
    children: ReactNode
}

export function LoginManager(props: LoginManagerProps): JSX.Element {
    const [loggedIn, setLoggedIn] = useState(false);
    
    function logIn(username: string, password: string) {
        setLoggedIn(true); 
        window.localStorage.setItem('username', username);
        window.localStorage.setItem('password', password);
    }
    
    function logOut() {
        setLoggedIn(false);
    }
    
    const context = {
        isLoggedIn: loggedIn,
        isAdmin: loggedIn,
        logIn: logIn,
        logOut: logOut,
    };
    
    return (
        <LoginContext.Provider value={context}>
            {props.children}
        </LoginContext.Provider>
    );
}