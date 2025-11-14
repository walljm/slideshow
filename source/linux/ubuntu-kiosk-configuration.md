
Install ubuntu.

## Ubuntu Frame Kiosk Setup on Ubuntu

```bash
sudo snap install ubuntu-frame
sudo snap install wpe-webkit-mir-kiosk
sudo snap set ubuntu-frame daemon=true
sudo snap connect wpe-webkit-mir-kiosk:wayland
sudo snap set wpe-webkit-mir-kiosk daemon=true
sudo snap set wpe-webkit-mir-kiosk url=http://localhost:5500
sudo reboot
```

After insalling the slideshow app, don't forget to enable the service.
```bash
sudo mkdir -p /opt/walljm/slideshow
```
Download the slideshow binary from https://github.com/walljm/slideshow/releases
and place it in `/opt/walljm/slideshow`.

```bash
cd /opt/walljm/slideshow
curl -LO <release_download_url>
tar -xvf ./slideshow*.tar.gz
sudo mv ./linux/slideshow.service /etc/systemd/system
sudo systemctl daemon-reload
sudo systemctl enable slideshow.service
sudo systemctl start slideshow.service
```
Install the service to switch to the kiosk tty on startup
```bash
sudo mv ./linux/kiosk.service /etc/systemd/system
sudo systemctl daemon-reload
sudo systemctl enable kiosk.service
sudo systemctl start kiosk.service
```

Configure samba auto mount.

FIRST!  Edit the file `source/linux/auto.smb` to replace `<ip_of_server>/path/to/photo/folder` with your samba server ip and photo folder path.
SECOND! Update the credential file at `/opt/walljm/slideshow/credential.txt` with your samba username and password.

Then run the following commands:
```bash
sudo apt install autofs cifs-utils
sudo cat >/etc/auto.master.d/slideshow.autofs <<EOF
/- /opt/walljm/slideshow/auto.smb --timeout 60 browse
EOF
```

Reboot!
```bash
sudo systemctl reboot
```