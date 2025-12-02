// Emergency Response Common JavaScript Functions

// Haversine Distance Calculator
function calculateDistance(lat1, lon1, lat2, lon2) {
    const R = 6371; // Radius of Earth in kilometers
    const dLat = toRadians(lat2 - lat1);
    const dLon = toRadians(lon2 - lon1);
    
    const a = Math.sin(dLat / 2) * Math.sin(dLat / 2) +
              Math.cos(toRadians(lat1)) * Math.cos(toRadians(lat2)) *
              Math.sin(dLon / 2) * Math.sin(dLon / 2);
    
    const c = 2 * Math.atan2(Math.sqrt(a), Math.sqrt(1 - a));
    const distance = R * c;
    
    return distance; // Returns distance in kilometers
}

function toRadians(degrees) {
    return degrees * (Math.PI / 180);
}

// Format distance for display
function formatDistance(km) {
    if (km < 1) {
        return (km * 1000).toFixed(0) + ' m';
    } else if (km < 10) {
        return km.toFixed(2) + ' km';
    } else {
        return km.toFixed(1) + ' km';
    }
}

// GPS Geolocation
function getCurrentLocation(successCallback, errorCallback) {
    if (!navigator.geolocation) {
        if (errorCallback) {
            errorCallback(new Error('Geolocation is not supported by this browser.'));
        }
        return;
    }

    navigator.geolocation.getCurrentPosition(
        function(position) {
            if (successCallback) {
                successCallback({
                    latitude: position.coords.latitude,
                    longitude: position.coords.longitude,
                    accuracy: position.coords.accuracy
                });
            }
        },
        function(error) {
            if (errorCallback) {
                let message = 'Unknown error occurred.';
                switch(error.code) {
                    case error.PERMISSION_DENIED:
                        message = 'Location permission denied. Please enable location access.';
                        break;
                    case error.POSITION_UNAVAILABLE:
                        message = 'Location information unavailable.';
                        break;
                    case error.TIMEOUT:
                        message = 'Location request timed out.';
                        break;
                }
                errorCallback(new Error(message));
            }
        },
        {
            enableHighAccuracy: true,
            timeout: 10000,
            maximumAge: 0
        }
    );
}

// GPS Accuracy Indicator
function getAccuracyClass(accuracy) {
    if (accuracy < 10) return 'gps-accuracy-good';
    if (accuracy < 50) return 'gps-accuracy-fair';
    return 'gps-accuracy-poor';
}

function getAccuracyText(accuracy) {
    if (accuracy < 10) return 'Excellent (±' + accuracy.toFixed(1) + 'm)';
    if (accuracy < 50) return 'Good (±' + accuracy.toFixed(1) + 'm)';
    return 'Fair (±' + accuracy.toFixed(1) + 'm)';
}

// Disaster Type Styling
function getDisasterBadgeClass(disasterType) {
    return 'badge-disaster-' + disasterType.toLowerCase().replace('_', '');
}

function getDisasterIcon(disasterType) {
    const icons = {
        'Flood': 'fa-water',
        'Earthquake': 'fa-house-damage',
        'Fire': 'fa-fire',
        'Hurricane': 'fa-wind',
        'Tornado': 'fa-tornado',
        'Tsunami': 'fa-water',
        'Landslide': 'fa-mountain',
        'Drought': 'fa-sun',
        'Wildfire': 'fa-fire-alt',
        'VolcanicEruption': 'fa-volcano',
        'Storm': 'fa-cloud-showers-heavy',
        'Cyclone': 'fa-wind',
        'Avalanche': 'fa-snowflake',
        'HeatWave': 'fa-temperature-high',
        'ColdWave': 'fa-temperature-low',
        'Epidemic': 'fa-disease',
        'Industrial': 'fa-industry',
        'Chemical': 'fa-flask',
        'Nuclear': 'fa-radiation',
        'Terrorism': 'fa-bomb',
        'CivilUnrest': 'fa-users',
        'BuildingCollapse': 'fa-building',
        'Explosion': 'fa-burst',
        'GasLeak': 'fa-smog',
        'OilSpill': 'fa-oil-can'
    };
    return icons[disasterType] || 'fa-exclamation-triangle';
}

// Severity Styling
function getSeverityClass(severity) {
    const severityMap = {
        1: 'severity-low',
        2: 'severity-moderate',
        3: 'severity-high',
        4: 'severity-critical',
        5: 'severity-catastrophic'
    };
    return severityMap[severity] || 'severity-low';
}

function getSeverityText(severity) {
    const textMap = {
        1: 'Low',
        2: 'Moderate',
        3: 'High',
        4: 'Critical',
        5: 'Catastrophic'
    };
    return textMap[severity] || 'Unknown';
}

function getSeverityColor(severity) {
    const colorMap = {
        1: 'green',
        2: 'yellow',
        3: 'orange',
        4: 'red',
        5: 'darkred'
    };
    return colorMap[severity] || 'gray';
}

// Status Styling
function getStatusBadgeClass(status) {
    return 'status-' + status.toLowerCase();
}

// Capacity Calculation
function calculateOccupancyPercent(current, total) {
    if (total === 0) return 0;
    return Math.round((current / total) * 100);
}

function getCapacityClass(percent) {
    if (percent < 70) return 'capacity-good';
    if (percent < 90) return 'capacity-limited';
    return 'capacity-full';
}

function getCapacityColor(percent) {
    if (percent < 70) return 'green';
    if (percent < 90) return 'orange';
    return 'red';
}

// Toast Notifications
function showToast(title, message, type = 'info') {
    const toastContainer = document.getElementById('toast-container');
    if (!toastContainer) return;

    const toastId = 'toast-' + Date.now();
    const toastClass = 'toast-' + type;
    
    const toastHtml = `
        <div id="${toastId}" class="toast toast-notification ${toastClass}" role="alert" aria-live="assertive" aria-atomic="true">
            <div class="toast-header">
                <strong class="me-auto">${title}</strong>
                <button type="button" class="btn-close" data-bs-dismiss="toast" aria-label="Close"></button>
            </div>
            <div class="toast-body">
                ${message}
            </div>
        </div>
    `;
    
    toastContainer.insertAdjacentHTML('beforeend', toastHtml);
    
    const toastElement = document.getElementById(toastId);
    const toast = new bootstrap.Toast(toastElement, {
        autohide: true,
        delay: 5000
    });
    toast.show();
    
    // Remove from DOM after hidden
    toastElement.addEventListener('hidden.bs.toast', function() {
        toastElement.remove();
    });
}

// Photo Gallery Lightbox
function initializePhotoGallery() {
    const galleryImages = document.querySelectorAll('.photo-gallery img');
    
    if (galleryImages.length === 0) return;
    
    // Create lightbox if it doesn't exist
    let lightbox = document.querySelector('.lightbox');
    if (!lightbox) {
        lightbox = document.createElement('div');
        lightbox.className = 'lightbox';
        lightbox.innerHTML = `
            <span class="lightbox-close">&times;</span>
            <img src="" alt="Full size image">
        `;
        document.body.appendChild(lightbox);
        
        // Close lightbox on click
        lightbox.querySelector('.lightbox-close').addEventListener('click', function() {
            lightbox.classList.remove('active');
        });
        lightbox.addEventListener('click', function(e) {
            if (e.target === lightbox) {
                lightbox.classList.remove('active');
            }
        });
    }
    
    // Add click handlers to gallery images
    galleryImages.forEach(function(img) {
        img.addEventListener('click', function() {
            lightbox.querySelector('img').src = this.src;
            lightbox.classList.add('active');
        });
    });
}

// File Upload Drag and Drop
function initializeFileUpload(dropZoneId, fileInputId, maxFiles = 5, maxSizeMB = 5) {
    const dropZone = document.getElementById(dropZoneId);
    const fileInput = document.getElementById(fileInputId);
    
    if (!dropZone || !fileInput) return;
    
    // Prevent defaults for drag events
    ['dragenter', 'dragover', 'dragleave', 'drop'].forEach(eventName => {
        dropZone.addEventListener(eventName, preventDefaults, false);
    });
    
    function preventDefaults(e) {
        e.preventDefault();
        e.stopPropagation();
    }
    
    // Highlight drop zone when dragging over
    ['dragenter', 'dragover'].forEach(eventName => {
        dropZone.addEventListener(eventName, function() {
            dropZone.classList.add('drag-over');
        }, false);
    });
    
    ['dragleave', 'drop'].forEach(eventName => {
        dropZone.addEventListener(eventName, function() {
            dropZone.classList.remove('drag-over');
        }, false);
    });
    
    // Handle dropped files
    dropZone.addEventListener('drop', function(e) {
        const dt = e.dataTransfer;
        const files = dt.files;
        handleFiles(files);
    }, false);
    
    // Handle file input change
    fileInput.addEventListener('change', function() {
        handleFiles(this.files);
    });
    
    function handleFiles(files) {
        if (files.length > maxFiles) {
            showToast('Too Many Files', `You can only upload up to ${maxFiles} files.`, 'warning');
            return;
        }
        
        const maxSizeBytes = maxSizeMB * 1024 * 1024;
        let validFiles = true;
        
        Array.from(files).forEach(file => {
            if (file.size > maxSizeBytes) {
                showToast('File Too Large', `${file.name} exceeds ${maxSizeMB}MB limit.`, 'warning');
                validFiles = false;
            }
            
            if (!file.type.match('image.*')) {
                showToast('Invalid File Type', `${file.name} is not an image file.`, 'warning');
                validFiles = false;
            }
        });
        
        if (!validFiles) {
            fileInput.value = '';
        }
    }
    
    // Click to browse
    dropZone.addEventListener('click', function() {
        fileInput.click();
    });
}

// Map Utilities
function initializeMap(mapId, centerLat = 6.5244, centerLng = 3.3792, zoom = 10) {
    const mapElement = document.getElementById(mapId);
    if (!mapElement) return null;
    
    const map = L.map(mapId).setView([centerLat, centerLng], zoom);
    
    L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
        attribution: '© OpenStreetMap contributors',
        maxZoom: 19
    }).addTo(map);
    
    return map;
}

function createCustomMarker(color, icon = 'fa-map-marker') {
    return L.divIcon({
        className: 'custom-marker',
        html: `<div style="background-color: ${color}; 
                          width: 30px; height: 30px; border-radius: 50%; 
                          border: 3px solid white; box-shadow: 0 0 5px rgba(0,0,0,0.5);
                          display: flex; align-items: center; justify-content: center;">
                    <i class="fas ${icon}" style="color: white; font-size: 14px;"></i>
               </div>`,
        iconSize: [30, 30],
        iconAnchor: [15, 15],
        popupAnchor: [0, -15]
    });
}

// Date/Time Formatting
function formatDateTime(dateString) {
    const date = new Date(dateString);
    const now = new Date();
    const diff = now - date;
    
    // If less than 1 hour, show minutes ago
    if (diff < 3600000) {
        const minutes = Math.floor(diff / 60000);
        return minutes === 0 ? 'Just now' : `${minutes} minute${minutes !== 1 ? 's' : ''} ago`;
    }
    
    // If less than 24 hours, show hours ago
    if (diff < 86400000) {
        const hours = Math.floor(diff / 3600000);
        return `${hours} hour${hours !== 1 ? 's' : ''} ago`;
    }
    
    // If less than 7 days, show days ago
    if (diff < 604800000) {
        const days = Math.floor(diff / 86400000);
        return `${days} day${days !== 1 ? 's' : ''} ago`;
    }
    
    // Otherwise show formatted date
    return date.toLocaleDateString() + ' ' + date.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' });
}

// Loading Spinner
function showLoadingSpinner(containerId) {
    const container = document.getElementById(containerId);
    if (!container) return;
    
    container.innerHTML = '<div class="loading-spinner"></div>';
}

function hideLoadingSpinner(containerId) {
    const container = document.getElementById(containerId);
    if (!container) return;
    
    container.innerHTML = '';
}

// Confirmation Dialog
function confirmAction(message, callback) {
    if (confirm(message)) {
        callback();
    }
}

// Initialize on page load
document.addEventListener('DOMContentLoaded', function() {
    // Initialize photo gallery if present
    initializePhotoGallery();
    
    // Initialize tooltips
    const tooltipTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="tooltip"]'));
    tooltipTriggerList.map(function(tooltipTriggerEl) {
        return new bootstrap.Tooltip(tooltipTriggerEl);
    });
});
