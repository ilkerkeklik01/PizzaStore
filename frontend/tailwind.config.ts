import type { Config } from 'tailwindcss'

export default {
  darkMode: ['class'],
  content: ['./index.html', './src/**/*.{ts,tsx}'],
  theme: {
    extend: {
      colors: {
        coal: {
          DEFAULT: '#1C1A17',
          light: '#2A2721',
          muted: '#3A352F',
        },
        parchment: {
          DEFAULT: '#F5ECD7',
          warm: '#FAF4E8',
          dark: '#EAD8B8',
        },
        terracotta: {
          DEFAULT: '#C44536',
          hover: '#B33C2F',
          light: '#D4614F',
          muted: 'rgba(196,69,54,0.12)',
        },
        ash: {
          DEFAULT: '#8B7E72',
          light: '#B5A89C',
          border: '#D5C8B4',
        },
      },
      fontFamily: {
        display: ['"Bodoni Moda"', 'Georgia', '"Times New Roman"', 'serif'],
        sans: ['"DM Sans"', 'system-ui', 'sans-serif'],
      },
      boxShadow: {
        card: '0 32px 64px rgba(0,0,0,0.55), 0 8px 24px rgba(0,0,0,0.3)',
        btn: '0 4px 16px rgba(196,69,54,0.38)',
        'btn-hover': '0 6px 22px rgba(196,69,54,0.5)',
      },
      keyframes: {
        slideUp: {
          '0%': { opacity: '0', transform: 'translateY(22px)' },
          '100%': { opacity: '1', transform: 'translateY(0)' },
        },
        fadeIn: {
          '0%': { opacity: '0' },
          '100%': { opacity: '1' },
        },
        spin: {
          '100%': { transform: 'rotate(360deg)' },
        },
      },
      animation: {
        'slide-up': 'slideUp 0.65s cubic-bezier(0.16,1,0.3,1) both',
        'fade-in': 'fadeIn 0.4s ease-out both',
      },
    },
  },
  plugins: [],
} satisfies Config
