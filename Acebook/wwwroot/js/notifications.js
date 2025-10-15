
// Log to confirm file is running
console.log("notifications.js loaded");

// Build the connection to your SignalR hub
const connection = new signalR.HubConnectionBuilder()
    .withUrl("/hubs/notifications")   // must match Program.cs route
    .withAutomaticReconnect()
    .build();



// Load stored notifications when the user logs in
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

// Call it right away once the connection is ready
connection.start()
    .then(() => {
        console.log("Connected to notification hub");
        const userId = document.body.dataset.userid;
        if (userId) connection.invoke("RegisterUserGroup", userId);
        loadStoredNotifications(); //new line that pulls stored notifications
    })
    .catch(err => console.error("SignalR connection failed:", err));


// Listen for notifications
connection.on("ReceiveNotification", (title, message) => {
    console.log("Notification:", title, message);
    showToast(title, message);
});

// Display a simple toast
function showToast(title, message) {
    const toast = document.createElement("div");
    toast.className =
        "fixed bottom-6 right-6 bg-teal-600 text-white px-4 py-2 rounded-lg shadow-lg opacity-100 transition-opacity duration-500";
    toast.innerHTML = `<strong>${title}</strong><br>${message}`;
    document.body.appendChild(toast);

    setTimeout(() => {
        toast.style.opacity = "0";
        setTimeout(() => toast.remove(), 500);
    }, 5000);
}
// ---------------------------
// Dropdown + badge behaviour
// ---------------------------
const bell = document.getElementById("notifBell");
const dropdown = document.getElementById("notifDropdown");
const badge = document.getElementById("notifBadge");
const list = document.getElementById("notifList");
const markAllBtn = document.getElementById("markAllRead");

if (bell && dropdown) {
    bell.addEventListener("click", () => {
        dropdown.classList.toggle("hidden");
        // Reset badge count when opened
        if (!dropdown.classList.contains("hidden")) {
            badge.classList.add("hidden");
            badge.textContent = "0";
            // Load stored notifications from server
            fetch("/notifications/unread")
                .then(res => res.json())
                .then(notifs => {
                    list.innerHTML = "";
                    notifs.forEach(n => renderNotifCard(n));
                });
        }
    });
}

// When the server sends a live notification
connection.on("ReceiveNotification", (title, message) => {
    showToast(title, message);
    bumpBadge();
    // Add to dropdown if open
    if (list) renderNotifCard({ title, message, createdOn: new Date() });
});

// Create small card for dropdown
function renderNotifCard(n) {
    const div = document.createElement("div");
    div.className = "border border-gray-200 rounded-lg p-2";
    div.innerHTML = `
    <div class="font-semibold text-gray-800">${n.title}</div>
    <div class="text-sm text-gray-700">${n.message}</div>
    <div class="text-xs text-gray-400">${new Date(n.createdOn).toLocaleString()}</div>`;
    list.prepend(div);
}

function bumpBadge() {
    if (!badge) return;
    const n = parseInt(badge.textContent || "0", 10) + 1;
    badge.textContent = String(n);
    badge.classList.remove("hidden");
}

// Mark all as read
if (markAllBtn) {
    markAllBtn.addEventListener("click", () => {
        fetch("/notifications/read-all", { method: "POST" })
            .then(() => {
                list.innerHTML = "";
                badge.classList.add("hidden");
            });
    });
}
