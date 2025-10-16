// transition.js

function playBubbleTransition(callback) {
    const overlay = document.getElementById("bubble-transition");

    // Remove old bubbles if any
    overlay.innerHTML = "";
    overlay.classList.add("active");

    const numBubbles = 80;
    for (let i = 0; i < numBubbles; i++) {
        const bubble = document.createElement("div");
        bubble.classList.add("bubble");

        // Randomize size, position, speed
        const size = Math.random() * 40 + 10; // 10px–50px
        const left = Math.random() * 100; // 0–100% across screen
        const duration = Math.random() * 2 + 3; // 3–5 seconds
        const delay = Math.random() * 1.5; // 0–1.5s

        bubble.style.width = `${size}px`;
        bubble.style.height = `${size}px`;
        bubble.style.left = `${left}%`;
        bubble.style.animationDuration = `${duration}s`;
        bubble.style.animationDelay = `${delay}s`;

        overlay.appendChild(bubble);
    }

    // Start transition
    overlay.style.opacity = "1";

    // Let bubbles rise for ~2.5s, then continue to next page
    setTimeout(() => {
        callback(); // redirect or submit

        // Keep bubbles visible while feed loads behind them
        setTimeout(() => {
            overlay.style.transition = "opacity 1.2s ease-out";
            overlay.style.opacity = "0";
            setTimeout(() => {
                overlay.classList.remove("active");
                overlay.innerHTML = "";
            }, 1200);
        }, 1200);
    }, 2500);
}

document.addEventListener("DOMContentLoaded", () => {
    const loginForm = document.querySelector("form#login-form");
    if (loginForm) {
        loginForm.addEventListener("submit", (e) => {
            e.preventDefault();
            const form = e.target;

            playBubbleTransition(() => {
                form.submit(); // continue form submit after animation
            });
        });
    }

    const signInButton = document.querySelector("#sign-in-btn");
    if (signInButton) {
        signInButton.addEventListener("click", (e) => {
            e.preventDefault();
            playBubbleTransition(() => {
                window.location.href = "/Home/NewsFeed";
            });
        });
    }

    // On feed page, add fade-in effect
    if (document.body.classList.contains("feed-fade")) {
        setTimeout(() => {
            document.body.classList.add("visible");
        }, 300); // small delay so the fade looks natural
    }
});


