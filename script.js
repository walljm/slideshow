class Slideshow {
    constructor() {
        this.files = [];
        this.currentIndex = 0;
        this.timer = null;
        this.currentContainer = 0;
        this.config = null;
        
        this.init();
    }

    async init() {
        try {
            await this.loadConfig();
            await this.loadFiles();
            this.hideLoading();
            
            if (this.files.length > 0 && this.config.autoStart) {
                this.startSlideshow();
            } else if (this.files.length === 0) {
                this.waitAndRetry();
            }
        } catch (error) {
            console.error('Initialization error:', error);
            this.showError('Failed to initialize slideshow: ' + error.message);
        }
    }

    async waitAndRetry() {
        console.log('No media files found, waiting 5 seconds before checking again...');
        this.showWaiting('No media files found. Checking again in 5 seconds...');
        
        setTimeout(async () => {
            try {
                await this.loadFiles();
                if (this.files.length > 0) {
                    this.hideWaiting();
                    if (this.config.autoStart) {
                        this.startSlideshow();
                    }
                } else {
                    this.waitAndRetry(); // Keep trying
                }
            } catch (error) {
                console.error('Error rechecking files:', error);
                this.waitAndRetry(); // Keep trying even on error
            }
        }, 5000);
    }

    async loadConfig() {
        try {
            const response = await fetch('/api/config');
            if (!response.ok) {
                throw new Error(`Failed to load config: ${response.status}`);
            }
            this.config = await response.json();
            console.log('Configuration loaded:', this.config);
        } catch (error) {
            console.error('Error loading configuration:', error);
            throw error;
        }
    }

    async loadFiles() {
        try {
            const response = await fetch('/api/files');
            if (!response.ok) {
                throw new Error(`Failed to load files: ${response.status}`);
            }
            this.files = await response.json();
            console.log(`Loaded ${this.files.length} files`);
        } catch (error) {
            console.error('Error loading files:', error);
            throw error;
        }
    }

    showLoading() {
        const loading = document.createElement('div');
        loading.className = 'loading';
        loading.textContent = 'Loading slideshow...';
        document.body.appendChild(loading);
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
        const mediaUrl = `/media/${encodeURIComponent(currentFile.name)}`;
        
        if (currentFile.type === 'video') {
            mediaElement = document.createElement('video');
            mediaElement.src = mediaUrl;
            mediaElement.autoplay = true;
            mediaElement.muted = false; // Enable sound
            mediaElement.controls = false;
            mediaElement.style.maxWidth = '100%';
            mediaElement.style.maxHeight = '100%';
            
            mediaElement.addEventListener('loadedmetadata', () => {
                this.switchToContainer(nextContainer);
                this.scheduleNext(mediaElement.duration * 1000);
            });
            
            mediaElement.addEventListener('ended', () => {
                this.nextSlide();
            });
            
            mediaElement.addEventListener('error', (e) => {
                console.error('Video error:', e);
                this.nextSlide(); // Skip problematic video
            });
        } else {
            mediaElement = document.createElement('img');
            mediaElement.src = mediaUrl;
            mediaElement.style.maxWidth = '100%';
            mediaElement.style.maxHeight = '100%';
            
            // Set animation duration to match image duration
            const animationDuration = `${this.config.imageDuration}s`;
            mediaElement.style.setProperty('--zoom-duration', animationDuration);
            
            // Alternate between zoom-in and zoom-out animations
            if (this.currentIndex % 2 === 0) {
                mediaElement.classList.add('zoom-in');
            } else {
                mediaElement.classList.add('zoom-out');
            }
            
            mediaElement.addEventListener('load', () => {
                this.switchToContainer(nextContainer);
                this.scheduleNext(this.config.imageDuration * 1000);
            });
            
            mediaElement.addEventListener('error', (e) => {
                console.error('Image error:', e);
                this.nextSlide(); // Skip problematic image
            });
        }
        
        containerElement.appendChild(mediaElement);
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
            this.nextSlide();
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
            // Reached the end, reload files and start over
            console.log('End of slideshow reached, reloading files...');
            try {
                await this.loadFiles();
                this.currentIndex = 0;
            } catch (error) {
                console.error('Error reloading files:', error);
                // Continue with existing files if reload fails
                this.currentIndex = 0;
            }
        }
        
        if (this.files.length > 0) {
            this.showCurrentSlide();
        }
    }
}

// Initialize slideshow when page loads
document.addEventListener('DOMContentLoaded', () => {
    new Slideshow();
});
