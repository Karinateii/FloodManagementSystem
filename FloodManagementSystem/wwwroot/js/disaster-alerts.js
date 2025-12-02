// SignalR client for real-time disaster alerts
const connection = new signalR.HubConnectionBuilder()
    .withUrl("/DisasterAlertHub")
    .withAutomaticReconnect()
    .configureLogging(signalR.LogLevel.Information)
    .build();

// Connection state handlers
connection.onreconnecting(() => {
    console.log("Attempting to reconnect to disaster alert hub...");
    showNotification("Reconnecting to disaster alerts...", "warning");
});

connection.onreconnected(() => {
    console.log("Reconnected to disaster alert hub");
    showNotification("Connected to real-time disaster alerts", "success");
    subscribeToUserLocations();
});

connection.onclose(() => {
    console.log("Disconnected from disaster alert hub");
    showNotification("Disconnected from disaster alerts", "error");
});

// Receive disaster alert for specific city
connection.on("ReceiveDisasterAlert", (alert) => {
    console.log("Received disaster alert:", alert);
    
    const message = `<strong>Disaster Alert!</strong><br/>
                     ${alert.City} - ${alert.Month} ${alert.Year}<br/>
                     ${alert.IsHighRisk ? '<span class="text-danger">High Risk</span>' : 'Low Risk'}`;
    
    showNotification(message, alert.IsHighRisk ? "danger" : "info", 10000);
    
    // Play alert sound for high risk
    if (alert.IsHighRisk) {
        playAlertSound();
    }
    
    // Update predictions table if visible
    if (typeof refreshPredictionsTable === 'function') {
        refreshPredictionsTable();
    }
});

// Receive LGA alert
connection.on("ReceiveLGAAlert", (alert) => {
    console.log("Received LGA alert:", alert);
    showNotification(alert.Message, "info", 8000);
});

// Receive broadcast alert
connection.on("ReceiveBroadcast", (alert) => {
    console.log("Received broadcast:", alert);
    showNotification(alert.Message, "warning", 12000);
    
    // Show modal for important broadcasts
    showBroadcastModal(alert.Message);
});

// Receive model training update
connection.on("ReceiveModelUpdate", (update) => {
    console.log("Model training update:", update);
    
    let message = `Model Training: ${update.Status}`;
    if (update.Accuracy) {
        message += `<br/>Accuracy: ${(update.Accuracy * 100).toFixed(2)}%`;
    }
    
    showNotification(message, "info", 5000);
    
    // Update training progress bar if visible
    if (typeof updateTrainingProgress === 'function') {
        updateTrainingProgress(update);
    }
});

// Start connection
async function startConnection() {
    try {
        await connection.start();
        console.log("Connected to disaster alert hub");
        showNotification("Connected to real-time disaster alerts", "success", 3000);
        
        // Subscribe to user's city and LGA
        await subscribeToUserLocations();
    } catch (err) {
        console.error("Error connecting to disaster alert hub:", err);
        showNotification("Failed to connect to disaster alerts. Retrying...", "error");
        
        // Retry connection after 5 seconds
        setTimeout(startConnection, 5000);
    }
}

// Subscribe to user's city and LGA groups
async function subscribeToUserLocations() {
    const cityId = document.body.getAttribute('data-user-city-id');
    const lgaId = document.body.getAttribute('data-user-lga-id');
    
    try {
        if (cityId) {
            await connection.invoke("SubscribeToCity", parseInt(cityId));
            console.log(`Subscribed to city ${cityId}`);
        }
        
        if (lgaId) {
            await connection.invoke("SubscribeToLGA", parseInt(lgaId));
            console.log(`Subscribed to LGA ${lgaId}`);
        }
    } catch (err) {
        console.error("Error subscribing to locations:", err);
    }
}

// Show notification using Bootstrap toast or alert
function showNotification(message, type = "info", duration = 5000) {
    // Try to use Bootstrap toast if available
    const toastContainer = document.getElementById('toast-container');
    
    if (toastContainer) {
        const toastId = 'toast-' + Date.now();
        const bgClass = `bg-${type}`;
        const textClass = type === 'warning' ? 'text-dark' : 'text-white';
        
        const toastHTML = `
            <div id="${toastId}" class="toast ${bgClass} ${textClass}" role="alert" aria-live="assertive" aria-atomic="true">
                <div class="toast-header ${bgClass} ${textClass}">
                    <strong class="me-auto">Disaster Alert System</strong>
                    <small>Just now</small>
                    <button type="button" class="btn-close btn-close-white" data-bs-dismiss="toast" aria-label="Close"></button>
                </div>
                <div class="toast-body">
                    ${message}
                </div>
            </div>
        `;
        
        toastContainer.insertAdjacentHTML('beforeend', toastHTML);
        const toastElement = document.getElementById(toastId);
        const toast = new bootstrap.Toast(toastElement, { delay: duration });
        toast.show();
        
        // Remove toast after it's hidden
        toastElement.addEventListener('hidden.bs.toast', () => {
            toastElement.remove();
        });
    } else {
        // Fallback to console if no toast container
        console.log(`Notification (${type}):`, message);
    }
}

// Show broadcast modal
function showBroadcastModal(message) {
    const modalHTML = `
        <div class="modal fade" id="broadcastModal" tabindex="-1" aria-labelledby="broadcastModalLabel" aria-hidden="true">
            <div class="modal-dialog modal-dialog-centered">
                <div class="modal-content">
                    <div class="modal-header bg-warning">
                        <h5 class="modal-title" id="broadcastModalLabel">
                            <i class="bi bi-megaphone"></i> System Broadcast
                        </h5>
                        <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close"></button>
                    </div>
                    <div class="modal-body">
                        ${message}
                    </div>
                    <div class="modal-footer">
                        <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">Close</button>
                    </div>
                </div>
            </div>
        </div>
    `;
    
    // Remove existing modal if present
    const existingModal = document.getElementById('broadcastModal');
    if (existingModal) {
        existingModal.remove();
    }
    
    document.body.insertAdjacentHTML('beforeend', modalHTML);
    const modal = new bootstrap.Modal(document.getElementById('broadcastModal'));
    modal.show();
    
    // Clean up modal after it's hidden
    document.getElementById('broadcastModal').addEventListener('hidden.bs.modal', function () {
        this.remove();
    });
}

// Play alert sound
function playAlertSound() {
    // Create audio element for alert
    const audio = new Audio('/sounds/alert.mp3');
    audio.volume = 0.5;
    audio.play().catch(err => console.log("Audio playback failed:", err));
}

// Initialize connection when document is ready
if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', startConnection);
} else {
    startConnection();
}

// Cleanup on page unload
window.addEventListener('beforeunload', async () => {
    if (connection.state === signalR.HubConnectionState.Connected) {
        await connection.stop();
    }
});
