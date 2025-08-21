class Slideshow {
  constructor() {
    this.files = [];
    this.currentIndex = 0;
    this.timer = null;
    this.currentContainer = 0;
    this.config = null;
    this.isPlaying = false;

    // Set up global functions for MAUI integration
    window.slideshow = this;
    window.handleApiResponse = this.handleApiResponse.bind(this);

    this.init();
  }

  async init() {
    try {
      this.log('Initializing config...');
      await this.loadConfig();
      this.log('Initializing files...');
      await this.loadFiles();

      if (this.files.length > 0) {
        this.startSlideshow(); // Always auto-start when files are loaded
      } else if (this.files.length === 0) {
        this.waitAndRetry();
      }
    } catch (error) {
      this.log('Failed to initialize slideshow: ' + error.message);
    }
  }

  async waitAndRetry() {
    this.showWaiting('No media files found. Checking again in 5 seconds...');

    setTimeout(async () => {
      try {
        await this.loadFiles();
        if (this.files.length > 0) {
          this.hideWaiting();
          this.startSlideshow(); // Always auto-start when files are found
        } else {
          this.waitAndRetry();
        }
      } catch (error) {
        this.log(`Error rechecking files: ${error}`);
        this.waitAndRetry();
      }
    }, 5000);
  }

  async loadConfig() {
    this.triggerApiCall('/api/config');

    return new Promise((resolve) => {
      this._configResolve = resolve;
    });
  }

  async loadFiles() {
    this.triggerApiCall('/api/files');

    return new Promise((resolve) => {
      this._filesResolve = resolve;
    });
  }

  triggerApiCall(endpoint) {
    this.log('triggering...' + endpoint);
    
    // Use a custom protocol that should definitely trigger navigation
    const customUrl = `slideshow-api:${endpoint}`;
    
    // Try multiple approaches to ensure navigation is triggered
    try {
      // Method 1: Direct navigation
      window.location.href = customUrl;
    } catch (e) {
      console.log('Method 1 failed, trying method 2');
      try {
        // Method 2: Create and click a link
        const link = document.createElement('a');
        link.href = customUrl;
        link.style.display = 'none';
        document.body.appendChild(link);
        link.click();
        document.body.removeChild(link);
      } catch (e2) {
        console.log('Method 2 failed, trying method 3');
        // Method 3: Use window.open
        window.open(customUrl, '_self');
      }
    }
  }

  handleApiResponse(endpoint, data) {
    try {
      const parsedData = typeof data === 'string' ? JSON.parse(data) : data;

      this.log(`parsed data: `,parsedData);
      if (endpoint === '/api/config') {
        this.config = parsedData;
        this.log('Configuration loaded:', this.config);
        if (this._configResolve) {
          this._configResolve();
          this._configResolve = null;
        }
      } else if (endpoint === '/api/files') {
        this.files = parsedData;
        this.log(`Loaded ${this.files.length} files:`, this.files);
        if (this._filesResolve) {
          this._filesResolve();
          this._filesResolve = null;
        }
      }
    } catch (error) {
      console.error('Error handling API response:', error);
    }
  }

  log(message, data) {
    //document.getElementById('log').textContent = message + ' ' + (data ? JSON.stringify(data) : '');
  }

  showWaiting(message) {
    this.hideWaiting();
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

    this.isPlaying = true;
    this.currentIndex = 0;
    this.showCurrentSlide();
  }

  stopSlideshow() {
    this.isPlaying = false;
    if (this.timer) {
      clearTimeout(this.timer);
      this.timer = null;
    }
  }

  showCurrentSlide() {
    if (this.files.length === 0 || !this.isPlaying) return;

    const currentFile = this.files[this.currentIndex];
    const nextContainer = this.currentContainer === 0 ? 1 : 0;
    const containerElement = document.getElementById(`mediaContainer${nextContainer + 1}`);

    containerElement.innerHTML = '';

    this.log(`Showing: ${currentFile.name} (${currentFile.type})`);

    let mediaElement;

    if (currentFile.type === 'video') {
      mediaElement = document.createElement('video');
      mediaElement.src = currentFile.path;
      mediaElement.autoplay = true;
      mediaElement.muted = false;
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
        this.nextSlide();
      });
    } else {
      mediaElement = document.createElement('img');
      mediaElement.src = currentFile.path;
      mediaElement.style.maxWidth = '100%';
      mediaElement.style.maxHeight = '100%';

      if (this.config && this.config.zoomOnImage) {
        const animationDuration = `${this.config.imageDuration}s`;
        mediaElement.style.setProperty('--zoom-duration', animationDuration);

        if (this.currentIndex % 2 === 0) {
          mediaElement.classList.add('zoom-in');
        } else {
          mediaElement.classList.add('zoom-out');
        }
      }

      mediaElement.addEventListener('load', () => {
        this.switchToContainer(nextContainer);
        this.scheduleNext(this.config.imageDuration * 1000);
      });

      mediaElement.addEventListener('error', (e) => {
        console.error('Image error:', e);
        this.nextSlide();
      });
    }

    containerElement.appendChild(mediaElement);
  }

  switchToContainer(containerIndex) {
    document.querySelectorAll('.media-container').forEach((container) => {
      container.classList.remove('active');
    });

    const activeContainer = document.getElementById(`mediaContainer${containerIndex + 1}`);
    if (activeContainer) {
      activeContainer.classList.add('active');
    }

    this.currentContainer = containerIndex;
  }

  scheduleNext(delay) {
    if (this.timer) {
      clearTimeout(this.timer);
    }

    if (this.isPlaying) {
      this.timer = setTimeout(() => {
        this.nextSlide();
      }, delay);
    }
  }

  nextSlide() {
    if (!this.isPlaying) return;

    this.currentIndex = (this.currentIndex + 1) % this.files.length;
    this.showCurrentSlide();
  }

  previousSlide() {
    if (!this.isPlaying) return;

    this.currentIndex = (this.currentIndex - 1 + this.files.length) % this.files.length;
    this.showCurrentSlide();
  }
}

// Initialize slideshow when DOM is loaded
document.addEventListener('DOMContentLoaded', () => {
  new Slideshow();
});
