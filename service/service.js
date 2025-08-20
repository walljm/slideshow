const Service = require('node-windows').Service;

// Create a new service object
const svc = new Service({
    name: 'Slideshow Server',
    description: 'A web-based slideshow application that serves media files',
    script: require('path').join(__dirname, '..', 'app', 'server.js'),
    nodeOptions: [
        '--harmony',
        '--max_old_space_size=4096'
    ],
    env: {
        name: "NODE_ENV",
        value: "production"
    }
});

// Listen for the "install" event, which indicates the
// process is available as a service.
svc.on('install', function(){
    console.log('Service installed successfully!');
    console.log('Starting service...');
    svc.start();
});

svc.on('start', function(){
    console.log('Service started successfully!');
    console.log('Slideshow server is now running as a Windows service');
    console.log('Access it at: http://localhost:3000');
});

svc.on('stop', function(){
    console.log('Service stopped');
});

svc.on('uninstall', function(){
    console.log('Service uninstalled successfully!');
});

svc.on('error', function(err){
    console.error('Service error:', err);
});

// Check command line arguments
const command = process.argv[2];

switch(command) {
    case 'install':
        console.log('Installing Slideshow Server as Windows service...');
        svc.install();
        break;
    
    case 'uninstall':
        console.log('Uninstalling Slideshow Server service...');
        svc.uninstall();
        break;
    
    case 'start':
        console.log('Starting Slideshow Server service...');
        svc.start();
        break;
    
    case 'stop':
        console.log('Stopping Slideshow Server service...');
        svc.stop();
        break;
    
    case 'restart':
        console.log('Restarting Slideshow Server service...');
        svc.restart();
        break;
    
    default:
        console.log('Slideshow Server Service Manager');
        console.log('Usage: node service.js [command]');
        console.log('');
        console.log('Commands:');
        console.log('  install   - Install the service');
        console.log('  uninstall - Remove the service');
        console.log('  start     - Start the service');
        console.log('  stop      - Stop the service');
        console.log('  restart   - Restart the service');
        console.log('');
        console.log('Example: node service.js install');
        break;
}
