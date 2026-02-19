/**
 * Triggers a file download in the browser from a byte array (base64).
 * Used by Blazor Server since there's no direct file-download API.
 *
 * @param {string} fileName - The name for the downloaded file.
 * @param {string} contentType - MIME type (e.g. "text/csv").
 * @param {string} base64Content - The file content as base64.
 */
window.downloadFile = (fileName, contentType, base64Content) => {
    const byteChars = atob(base64Content);
    const byteNumbers = new Uint8Array(byteChars.length);
    for (let i = 0; i < byteChars.length; i++) {
        byteNumbers[i] = byteChars.charCodeAt(i);
    }
    const blob = new Blob([byteNumbers], { type: contentType });
    const url = URL.createObjectURL(blob);

    const a = document.createElement('a');
    a.href = url;
    a.download = fileName;
    document.body.appendChild(a);
    a.click();
    document.body.removeChild(a);

    URL.revokeObjectURL(url);
};