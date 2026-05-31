// Default config used until /api/config responds. Keeps the slideshow alive
// even if the server is unreachable on first load.
const DEFAULT_CONFIG = {
    imageDuration: 5,
    fadeTransitionDuration: 1,
    zoomOnImage: true,
    displayOrder: 'Alpha',
};

// How often to refresh the file list while running (ms). The server-side
// MediaSyncService mirrors SMB on a timer; this picks up new media without
// requiring a page reload.
const FILE_LIST_REFRESH_MS = 5 * 60 * 1000;

// Fetch timeout - server may be frozen; never wait forever.
const FETCH_TIMEOUT_MS = 10 * 1000;

// Backoff schedule for failed fetches (ms). Caps at the last value.
const RETRY_BACKOFF_MS = [2000, 5000, 10000, 30000, 60000];

// Watchdog: if no slide advances in this multiple of imageDuration, recover.
const WATCHDOG_MULTIPLIER = 4;
const WATCHDOG_MIN_MS = 20 * 1000;

// Consecutive media load failures after which we force a page reload.
const MAX_CONSECUTIVE_MEDIA_ERRORS = 25;

async function fetchJsonWithTimeout(url, timeoutMs) {
    const controller = new AbortController();
    const timer = setTimeout(() => controller.abort(), timeoutMs);
    try {
        const response = await fetch(url, { signal: controller.signal, cache: 'no-store' });
        if (!response.ok) {
            throw new Error(`HTTP ${response.status}`);
        }
        return await response.json();
    } finally {
        clearTimeout(timer);
    }
}

function backoffFor(attempt) {
    const i = Math.min(attempt, RETRY_BACKOFF_MS.length - 1);
    return RETRY_BACKOFF_MS[i];
}

class Slideshow {
    constructor() {
        this.files = [];
        this.currentIndex = 0;
        this.timer = null;
        this.currentContainer = 0;
        this.config = DEFAULT_CONFIG;
        this.lastAdvanceAt = Date.now();
        this.consecutiveMediaErrors = 0;
        this.fileRefreshTimer = null;
        this.watchdogTimer = null;

        this.init().then(() => {});
    }

    async init() {
        // Always proceed - config and files retry forever in the background.
        await this.loadConfigWithRetry();
        await this.loadFilesUntilNonEmpty();
        this.hideLoading();
        this.hideWaiting();
        this.startSlideshow();
        this.startFileRefresh();
        this.startWatchdog();
    }

    async loadConfigWithRetry() {
        let attempt = 0;
        while (true) {
            try {
                const cfg = await fetchJsonWithTimeout('/api/config', FETCH_TIMEOUT_MS);
                this.config = { ...DEFAULT_CONFIG, ...cfg };
                console.log('Configuration loaded:', this.config);
                return;
            } catch (error) {
                const wait = backoffFor(attempt++);
                console.warn(`Config load failed (${error.message}); retrying in ${wait}ms`);
                this.showWaiting(`Waiting for server (config)...`);
                await new Promise(r => setTimeout(r, wait));
            }
        }
    }

    async loadFilesUntilNonEmpty() {
        let attempt = 0;
        while (true) {
            try {
                const files = await fetchJsonWithTimeout('/api/files', FETCH_TIMEOUT_MS);
                if (Array.isArray(files) && files.length > 0) {
                    this.files = files;
                    console.log(`Loaded ${this.files.length} files`);
                    return;
                }
                this.showWaiting('No media files available yet...');
            } catch (error) {
                console.warn(`Files load failed (${error.message})`);
                this.showWaiting('Waiting for server (files)...');
            }
            const wait = backoffFor(attempt++);
            await new Promise(r => setTimeout(r, wait));
        }
    }

    // Best-effort: refresh the file list periodically. Failures are swallowed
    // so the running slideshow is never disturbed.
    async refreshFilesQuiet() {
        try {
            const files = await fetchJsonWithTimeout('/api/files', FETCH_TIMEOUT_MS);
            if (Array.isArray(files) && files.length > 0) {
                this.files = files;
                console.log(`Refreshed file list: ${this.files.length} files`);
            }
        } catch (error) {
            console.warn(`File refresh skipped (${error.message})`);
        }
    }

    startFileRefresh() {
        if (this.fileRefreshTimer) clearInterval(this.fileRefreshTimer);
        this.fileRefreshTimer = setInterval(() => {
            this.refreshFilesQuiet();
        }, FILE_LIST_REFRESH_MS);
    }

    // If the slideshow has not advanced in a long time, something is wedged
    // (frozen video, dead image, lost timer). Recover by skipping forward;
    // if that also fails repeatedly, the media-error path will reload the page.
    startWatchdog() {
        if (this.watchdogTimer) clearInterval(this.watchdogTimer);
        this.watchdogTimer = setInterval(() => {
            const maxIdle = Math.max(
                WATCHDOG_MIN_MS,
                (this.config.imageDuration || 5) * 1000 * WATCHDOG_MULTIPLIER,
            );
            if (Date.now() - this.lastAdvanceAt > maxIdle) {
                console.warn('Watchdog: no slide advance detected; forcing next slide');
                this.lastAdvanceAt = Date.now();
                this.nextSlide().then(() => {});
            }
        }, 5000);
    }

    hideLoading() {
        const loading = document.querySelector('.loading');
        if (loading) {
            loading.remove();
        }
    }

    showError(message) {
        this.hideLoading();
        const error = document.createElement('div');
        error.className = 'error';
        error.innerHTML = `<div>Error: ${message}</div><div style="margin-top: 10px; font-size: 14px;">Check the console for more details</div>`;
        document.body.appendChild(error);
    }

    showWaiting(message) {
        this.hideLoading();
        this.hideWaiting(); // Remove any existing waiting message
        const waiting = document.createElement('div');
        waiting.className = 'waiting';
        waiting.textContent = message;
        document.body.appendChild(waiting);
    }

    hideWaiting() {
        const waiting = document.querySelector('.waiting');
        if (waiting) {
            waiting.remove();
        }
    }

    startSlideshow() {
        if (this.files.length === 0) return;

        this.currentIndex = 0;
        this.showCurrentSlide();
    }

    showCurrentSlide() {
        if (this.files.length === 0) return;

        const currentFile = this.files[this.currentIndex];
        const nextContainer = this.currentContainer === 0 ? 1 : 0;
        const containerElement = document.getElementById(`mediaContainer${nextContainer + 1}`);

        // Clear previous content
        containerElement.innerHTML = '';

        console.log(`Showing: ${currentFile.name} (${currentFile.type})`);

        let mediaElement;
        const mediaUrl = `media/${encodeURIComponent(currentFile.name)}`;

        if (currentFile.type === 'video') {
            mediaElement = document.createElement('video');
            mediaElement.src = mediaUrl;
            mediaElement.autoplay = true;
            mediaElement.muted = true; // Mute to allow autoplay in modern browsers
            mediaElement.controls = false;
            mediaElement.style.maxWidth = '100%';
            mediaElement.style.maxHeight = '100%';
            mediaElement.preload = 'auto';
            mediaElement.playsInline = true; // Helps with mobile autoplay
            mediaElement.setAttribute('playsinline', ''); // Additional mobile support

            mediaElement.addEventListener('loadedmetadata', () => {
                this.onSlideAdvanced();
                this.switchToContainer(nextContainer);
                // Try to play the video explicitly
                this.playVideo(mediaElement);
            });

            // Also try to play when the video is loaded enough to start playing
            mediaElement.addEventListener('canplay', () => {
                this.playVideo(mediaElement);
            });

            mediaElement.addEventListener('ended', () => {
                this.nextSlide().then(() => {});
            });

            mediaElement.addEventListener('error', (e) => {
                console.error('Video error:', e);
                this.handleMediaError();
                this.nextSlide().then(() => {
                }); // Skip problematic video
            });
        } else {
            mediaElement = document.createElement('img');
            mediaElement.src = mediaUrl;
            mediaElement.style.maxWidth = '100%';
            mediaElement.style.maxHeight = '100%';

            // Only apply zoom animation if enabled in config
            if (this.config.zoomOnImage) {
                // Set animation duration to match image duration
                const animationDuration = `${this.config.imageDuration}s`;
                mediaElement.style.setProperty('--zoom-duration', animationDuration);

                // Alternate between zoom-in and zoom-out animations
                if (this.currentIndex % 2 === 0) {
                    mediaElement.classList.add('zoom-in');
                } else {
                    mediaElement.classList.add('zoom-out');
                }
            }

            mediaElement.addEventListener('load', () => {
                this.onSlideAdvanced();
                this.switchToContainer(nextContainer);
                this.scheduleNext(this.config.imageDuration * 1000);
            });

            mediaElement.addEventListener('error', (e) => {
                console.error('Image error:', e);
                this.handleMediaError();
                this.nextSlide().then(() => {}); // Skip problematic image
            });
        }

        containerElement.appendChild(mediaElement);
    }

    async playVideo(videoElement) {
        try {
            await videoElement.play();
        } catch (error) {
            console.error('Failed to autoplay video:', error);
            // If autoplay fails, try unmuting and playing again after user interaction
            if (error.name === 'NotAllowedError') {
                console.log('Autoplay prevented - video is ready but requires user interaction');
            }
        }
    }

    switchToContainer(containerIndex) {
        // Remove active class from all containers
        document.querySelectorAll('.media-container').forEach(container => {
            container.classList.remove('active');
        });

        // Add active class to current container
        document.getElementById(`mediaContainer${containerIndex + 1}`).classList.add('active');
        this.currentContainer = containerIndex;
    }

    scheduleNext(duration) {
        this.clearTimer();

        this.timer = setTimeout(() => {
            this.nextSlide().then(() => {});
        }, duration);
    }

    clearTimer() {
        if (this.timer) {
            clearTimeout(this.timer);
            this.timer = null;
        }
    }

    async nextSlide() {
        this.currentIndex++;

        if (this.currentIndex >= this.files.length) {
            // Reached the end - try to refresh the list but never block on it.
            console.log('End of slideshow reached, refreshing files...');
            await this.refreshFilesQuiet();
            this.currentIndex = 0;
        }

        if (this.files.length > 0) {
            this.showCurrentSlide();
        } else {
            // No files at all - poll quietly and resume when something arrives.
            this.showWaiting('No media files available yet...');
            setTimeout(() => this.nextSlide(), 5000);
        }
    }

    onSlideAdvanced() {
        this.lastAdvanceAt = Date.now();
        this.consecutiveMediaErrors = 0;
        this.hideWaiting();
    }

    handleMediaError() {
        this.consecutiveMediaErrors++;
        if (this.consecutiveMediaErrors >= MAX_CONSECUTIVE_MEDIA_ERRORS) {
            console.warn(
                `Too many consecutive media errors (${this.consecutiveMediaErrors}); reloading page`,
            );
            // Last-ditch recovery: full page reload re-fetches config and files.
            window.location.reload();
        }
    }
}

// Initialize slideshow when page loads
document.addEventListener('DOMContentLoaded', () => {
    new Slideshow();
});
