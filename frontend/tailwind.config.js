/** @type {import('tailwindcss').Config} */
export default {
  content: [
    "./index.html",
    "./src/**/*.{js,ts,jsx,tsx}",
  ],
  theme: {
    extend: {
      colors: {
        'bank-blue': '#0047AB',
        'bank-dark': '#001F4D',
      }
    },
  },
  plugins: [],
}