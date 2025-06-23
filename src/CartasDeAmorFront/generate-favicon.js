/* SVG favicon data for a Love Letter */
const svgData = `
<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 100 100" width="32" height="32">
  <rect width="100" height="100" rx="10" fill="#9c27b0" />
  <path d="M50 25 L80 45 L80 80 L20 80 L20 45 Z" fill="white" />
  <path d="M50 25 L20 45 L80 45 Z" fill="white" />
  <path d="M35 55 Q50 70 65 55 Q60 45 50 50 Q40 45 35 55 Z" fill="#e91e63" />
</svg>
`;

// Convert SVG to data URL
const dataUrl = `data:image/svg+xml;base64,${btoa(svgData)}`;

// Create a link to download the favicon
const link = document.createElement('a');
link.href = dataUrl;
link.download = 'favicon.svg';
document.body.appendChild(link);
link.click();
document.body.removeChild(link);

// Instructions
console.log('Favicon generated! You can use this SVG file as your favicon.');
console.log('Place it in the static folder and update the href in app.html.');
