// مسیر پیشنهادی: src/HotelReservation.Client/wwwroot/js/fileUtils.js

// این تابع یک نام فایل و محتوای آن را به صورت رشته Base64 دریافت کرده
// و یک لینک موقت برای دانلود آن ایجاد و کلیک می‌کند.
function downloadFileFromStream(fileName, contentStreamReference) {
    const a = document.createElement('a');
    a.href = contentStreamReference;
    a.download = fileName;
    a.click();
    a.remove();
}

// این تابع یک نام فایل و آرایه بایت را دریافت کرده و آن را برای دانلود آماده می‌کند.
// این روش برای فایل‌های بزرگتر بهتر است.
async function downloadFileFromByteArray(fileName, byteData) {
    // ایجاد یک Blob از آرایه بایت
    const blob = new Blob([byteData], { type: "application/octet-stream" });
    
    // ایجاد یک URL موقت برای Blob
    const url = URL.createObjectURL(blob);
    
    // ایجاد یک لینک مخفی و شبیه‌سازی کلیک روی آن
    const a = document.createElement('a');
    document.body.appendChild(a);
    a.style = "display: none";
    a.href = url;
    a.download = fileName;
    a.click();
    
    // پاک کردن لینک و URL موقت
    window.URL.revokeObjectURL(url);
    document.body.removeChild(a);
}
