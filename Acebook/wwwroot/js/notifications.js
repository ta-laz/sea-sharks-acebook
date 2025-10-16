// Log to confirm file is running
console.log("notifications.js loaded");

// ---------------------------
// Element references (defined FIRST so they're available everywhere)
// ---------------------------
const bell = document.getElementById("notifBell");
const dropdown = document.getElementById("notifDropdown");
const badge = document.getElementById("notifBadge");
const list = document.getElementById("notifList");
const markAllBtn = document.getElementById("markAllRead");

// ---------------------------
// Build SignalR connection
// ---------------------------
const connection = new signalR.HubConnectionBuilder()
    .withUrl("/hubs/notifications")
    .withAutomaticReconnect()
    .build();

// ---------------------------
// Load stored notifications on login
// ---------------------------
async function loadStoredNotifications() {
    try {
        const res = await fetch("/notifications/unread");
        if (!res.ok) throw new Error("Failed to load notifications");
        const notifs = await res.json();

        // Update the badge count
        if (notifs.length > 0 && badge) {
            badge.textContent = String(notifs.length);
            badge.classList.remove("hidden");
        }

        // Fill dropdown list
        if (list) {
            list.innerHTML = "";
            notifs.forEach(n => renderNotifCard(n));
        }
    } catch (err) {
        console.error("Could not load notifications:", err);
    }
}

// ---------------------------
// Start connection
// ---------------------------
connection.start()
    .then(() => {
        console.log("Connected to notification hub");
        const userId = document.body.dataset.userid;
        if (userId) connection.invoke("RegisterUserGroup", userId);
        loadStoredNotifications();
    })
    .catch(err => console.error("SignalR connection failed:", err));

// ---------------------------
// Single unified listener
// ---------------------------
connection.on("ReceiveNotification", (title, message, url) => {
    console.log("Notification received:", title, message, url);

    showToast(title, message, url); // popup
    bumpBadge();                    // red badge increment
    if (list) renderNotifCard({ title, message, url, createdOn: new Date() }); // dropdown
});

// ---------------------------
// UI functions
// ---------------------------

// Toast popup
function showToast(title, message, url) {
    const toast = document.createElement("div");
    toast.className =
        "fixed bottom-6 right-6 bg-teal-600 text-white px-4 py-2 rounded-lg shadow-lg opacity-100 transition-opacity duration-500 cursor-pointer";
    toast.innerHTML = `<strong>${title}</strong><br>${message}`;
    document.body.appendChild(toast);

    if (url) {
        toast.addEventListener("click", () => (window.location.href = url));
    }

    setTimeout(() => {
        toast.style.opacity = "0";
        setTimeout(() => toast.remove(), 500);
    }, 5000);
}

// Dropdown and badge
if (bell && dropdown) {
    bell.addEventListener("click", () => {
        dropdown.classList.toggle("hidden");

        // Reset badge when opened
        if (!dropdown.classList.contains("hidden")) {
            badge.classList.add("hidden");
            badge.textContent = "0";

            // Reload latest notifications
            fetch("/notifications/unread")
                .then(res => res.json())
                .then(notifs => {
                    list.innerHTML = "";
                    notifs.forEach(n => renderNotifCard(n));
                });
        }
    });
}

// Close dropdown if clicking outside it
document.addEventListener("click", (e) => {
    // if dropdown or bell don't exist, skip
    if (!dropdown || !bell) return;

    // check if click was outside both the dropdown and the bell
    if (!dropdown.classList.contains("hidden") && !dropdown.contains(e.target) && !bell.contains(e.target)) {
        dropdown.classList.add("hidden");
    }
});

// Card renderer
function renderNotifCard(n) {
    const div = document.createElement("div");
    div.className =
        "border border-gray-200 rounded-lg p-2 hover:bg-gray-50 cursor-pointer";
    div.innerHTML = `
    <div class="font-semibold text-gray-800">${n.title}</div>
    <div class="text-sm text-gray-700">${n.message}</div>
    <div class="text-xs text-gray-400">${new Date(
        n.createdOn
    ).toLocaleString()}</div>
  `;

    if (n.url) {
        div.addEventListener("click", () => {
            window.location.href = n.url;
        });
    }

    list.prepend(div);
}

// Red badge counter
function bumpBadge() {
    if (!badge) return;
    const n = parseInt(badge.textContent || "0", 10) + 1;
    badge.textContent = String(n);
    badge.classList.remove("hidden");
}

// Mark all as read
if (markAllBtn) {
    markAllBtn.addEventListener("click", () => {
        fetch("/notifications/read-all", { method: "POST" }).then(() => {
            list.innerHTML = "";
            badge.classList.add("hidden");
        });
    });
}
