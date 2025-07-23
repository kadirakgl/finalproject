// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

// SignalR ile gerçek zamanlı bildirimler
if (window.SignalR === undefined) {
    var script = document.createElement('script');
    script.src = 'https://cdnjs.cloudflare.com/ajax/libs/microsoft-signalr/3.1.18/signalr.min.js';
    script.onload = setupSignalRNotification;
    document.head.appendChild(script);
} else {
    setupSignalRNotification();
}

function setupSignalRNotification() {
    if (!window.signalRConnected) {
        const connection = new signalR.HubConnectionBuilder()
            .withUrl('/notificationHub')
            .build();

        connection.on('ReceiveNotification', function (message) {
            // Basit bir toast/alert ile göster
            if (window.toastr) {
                toastr.info(message, 'Bildirim');
            } else {
                alert('Bildirim: ' + message);
            }
        });

        connection.start().then(function () {
            window.signalRConnected = true;
        }).catch(function (err) {
            console.error('SignalR bağlantı hatası:', err.toString());
        });
    }
}
