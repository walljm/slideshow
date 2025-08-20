const http = require('http');
const fs = require('fs');
const path = require('path');

const port = 3000;

// Hardcoded supported file extensions (not configurable)
const SUPPORTED_IMAGE_EXTENSIONS = ["jpg", "jpeg", "png", "gif", "webp", "bmp", "svg"];
const SUPPORTED_VIDEO_EXTENSIONS = ["mp4", "webm", "ogg", "avi", "mov"];
const SUPPORTED_EXTENSIONS = [...SUPPORTED_IMAGE_EXTENSIONS, ...SUPPORTED_VIDEO_EXTENSIONS];

// Load configuration
let config;
try {
    const configData = fs.readFileSync('./config.json', 'utf8');
    config = JSON.parse(configData);
    console.log('Configuration loaded:', config);
} catch (error) {
    console.error('Error loading config.json:', error.message);
    console.log('Using default configuration');
    config = {
        imageDuration: 5,
        folderPath: './media',
        fadeTransitionDuration: 1,
        autoStart: true,
        zoomOnImage: true
    };
}

const mimeTypes = {
    '.html': 'text/html',
    '.css': 'text/css',
    '.js': 'application/javascript',
    '.json': 'application/json',
    '.png': 'image/png',
    '.jpg': 'image/jpeg',
    '.jpeg': 'image/jpeg',
    '.gif': 'image/gif',
    '.svg': 'image/svg+xml',
    '.webp': 'image/webp',
    '.bmp': 'image/bmp',
    '.mp4': 'video/mp4',
    '.webm': 'video/webm',
    '.ogg': 'video/ogg',
    '.avi': 'video/avi',
    '.mov': 'video/quicktime'
};

// Function to get media files from the configured folder
function getMediaFiles() {
    const files = [];
    
    try {
        if (!fs.existsSync(config.folderPath)) {
            console.error(`Configured folder path does not exist: ${config.folderPath}`);
            return [];
        }
        
        const entries = fs.readdirSync(config.folderPath);
        
        for (const entry of entries) {
            const fullPath = path.join(config.folderPath, entry);
            const stats = fs.statSync(fullPath);
            
            if (stats.isFile()) {
                const extension = path.extname(entry).toLowerCase().substring(1);
                if (SUPPORTED_EXTENSIONS.includes(extension)) {
                    files.push({
                        name: entry,
                        path: fullPath,
                        type: SUPPORTED_VIDEO_EXTENSIONS.includes(extension) ? 'video' : 'image'
                    });
                }
            }
        }
        
        // Sort files by name
        files.sort((a, b) => a.name.localeCompare(b.name));
        console.log(`Found ${files.length} media files in ${config.folderPath}`);
        
    } catch (error) {
        console.error('Error reading media folder:', error.message);
    }
    
    return files;
}

const server = http.createServer((req, res) => {
    const url = new URL(req.url, `http://localhost:${port}`);
    
    // CORS headers for development
    res.setHeader('Access-Control-Allow-Origin', '*');
    res.setHeader('Access-Control-Allow-Methods', 'GET, POST, OPTIONS');
    res.setHeader('Access-Control-Allow-Headers', 'Content-Type');
    
    if (req.method === 'OPTIONS') {
        res.writeHead(200);
        res.end();
        return;
    }
    
    // API endpoints
    if (url.pathname === '/api/config') {
        res.writeHead(200, { 'Content-Type': 'application/json' });
        res.end(JSON.stringify(config));
        return;
    }
    
    if (url.pathname === '/api/files') {
        const files = getMediaFiles();
        res.writeHead(200, { 'Content-Type': 'application/json' });
        res.end(JSON.stringify(files));
        return;
    }
    
    // Serve media files
    if (url.pathname.startsWith('/media/')) {
        const fileName = decodeURIComponent(url.pathname.substring(7)); // Remove '/media/'
        const filePath = path.join(config.folderPath, fileName);
        
        // Security check: ensure the file is within the configured folder
        const resolvedPath = path.resolve(filePath);
        const resolvedFolder = path.resolve(config.folderPath);
        
        if (!resolvedPath.startsWith(resolvedFolder)) {
            res.writeHead(403);
            res.end('Access denied');
            return;
        }
        
        fs.readFile(filePath, (error, content) => {
            if (error) {
                res.writeHead(404);
                res.end('File not found');
            } else {
                const extname = String(path.extname(filePath)).toLowerCase();
                const mimeType = mimeTypes[extname] || 'application/octet-stream';
                
                res.writeHead(200, { 'Content-Type': mimeType });
                res.end(content);
            }
        });
        return;
    }
    
    // Serve static files
    let filePath = '.' + url.pathname;
    
    // Default to index.html
    if (filePath === './') {
        filePath = './index.html';
    }
    
    const extname = String(path.extname(filePath)).toLowerCase();
    const mimeType = mimeTypes[extname] || 'application/octet-stream';
    
    fs.readFile(filePath, (error, content) => {
        if (error) {
            if (error.code === 'ENOENT') {
                res.writeHead(404);
                res.end('File not found');
            } else {
                res.writeHead(500);
                res.end('Server error: ' + error.code);
            }
        } else {
            res.writeHead(200, { 'Content-Type': mimeType });
            res.end(content, 'utf-8');
        }
    });
});

server.listen(port, () => {
    console.log(`Slideshow server running at http://localhost:${port}/`);
    console.log(`Serving media from: ${config.folderPath}`);
    console.log('Open this URL in your browser to start the slideshow');
});
