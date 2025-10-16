
function playBubbleTransition(callback) {
    const overlay = document.getElementById("bubble-transition");

    // Remove old bubbles if any
    overlay.innerHTML = "";
    overlay.classList.add("active");

    const numBubbles = 100;
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

function initSignupForm() {
    const signUpForm = document.getElementById("signup-form");    
    if (!signUpForm) return;

    signUpForm.addEventListener("submit", async (e) => {
        e.preventDefault();

        const formData = new FormData(signUpForm);

        try {
            const response = await fetch(signUpForm.action, {
                method: "POST",
                body: formData,
                headers: {
                    "RequestVerificationToken": document.querySelector('input[name="__RequestVerificationToken"]').value
                }
            });

            if (response.redirected) {
                playBubbleTransition(() => {
                    window.location.href = response.url;
                });
            } else {
                const html = await response.text();
                document.body.innerHTML = html;

                // ✅ Reinitialize the form JS after replacing HTML
                initSignupForm();
            }
        } catch (err) {
            console.error(err);
            alert("An error occurred while signing up.");
        }
    });
}

// Initialize on first page load
document.addEventListener("DOMContentLoaded", initSignupForm);



