// UI Components Index
// This file provides a single entry point for importing UI components

export { default as Button } from './Button.svelte';
export { default as Modal } from './Modal.svelte';
export { default as Input } from './Input.svelte';
export { default as Select } from './Select.svelte';
export { default as Checkbox } from './Checkbox.svelte';
export { default as LoadingSpinner } from './LoadingSpinner.svelte';
export { default as Card } from './Card.svelte';
export { default as AnimationToggle } from './AnimationToggle.svelte';
export { default as LanguageSwitcher } from './LanguageSwitcher.svelte';

// Types for component props (can be expanded as needed)
export interface ButtonProps {
  variant?: 'primary' | 'secondary' | 'success' | 'danger' | 'warning' | 'info' | 'light' | 'dark';
  size?: 'sm' | 'md' | 'lg';
  disabled?: boolean;
  loading?: boolean;
  type?: 'button' | 'submit' | 'reset';
  fullWidth?: boolean;
  outline?: boolean;
}

export interface ModalProps {
  isOpen?: boolean;
  title?: string;
  size?: 'sm' | 'md' | 'lg' | 'xl';
  closable?: boolean;
  backdrop?: boolean;
  theme?: 'default' | 'game';
}

export interface InputProps {
  type?: 'text' | 'email' | 'password' | 'number' | 'tel' | 'url' | 'search';
  value?: string;
  placeholder?: string;
  label?: string;
  id?: string;
  required?: boolean;
  disabled?: boolean;
  readonly?: boolean;
  error?: string;
  success?: string;
  helpText?: string;
  size?: 'sm' | 'md' | 'lg';
  fullWidth?: boolean;
}
