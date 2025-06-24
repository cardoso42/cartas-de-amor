// navigationGuard.ts - Utility for handling navigation and page lifecycle events
import { browser } from '$app/environment';
import { beforeNavigate, afterNavigate } from '$app/navigation';

export interface NavigationGuardOptions {
  // Async cleanup function called before navigation
  onBeforeNavigate?: (reason: 'navigation' | 'page_close' | 'manual') => Promise<void>;
  
  // Sync cleanup function called on page unload
  onPageUnload?: (reason: 'navigation' | 'page_close' | 'manual') => void;
  
  // Function called after navigation completes
  onAfterNavigate?: () => void;
  
  // Function called when tab visibility changes
  onVisibilityChange?: (isVisible: boolean) => void;
  
  // Whether to show a confirmation dialog on page unload
  confirmBeforeLeave?: boolean;
  
  // Custom confirmation message
  confirmMessage?: string;
}

export class NavigationGuard {
  private options: NavigationGuardOptions;
  private hasExecutedCleanup = false;
  private beforeUnloadHandler: ((event: BeforeUnloadEvent) => void) | null = null;
  private visibilityChangeHandler: (() => void) | null = null;

  constructor(options: NavigationGuardOptions = {}) {
    this.options = options;
    this.setupNavigationGuards();
  }

  private setupNavigationGuards() {
    // Setup SvelteKit navigation guards
    beforeNavigate(async (navigation) => {
      console.log('Navigation guard: beforeNavigate triggered', navigation.to?.url.pathname);
      
      if (this.options.onBeforeNavigate && !this.hasExecutedCleanup) {
        try {
          await this.options.onBeforeNavigate('navigation');
          this.hasExecutedCleanup = true;
        } catch (error) {
          console.error('Error in beforeNavigate cleanup:', error);
        }
      }
    });

    afterNavigate((navigation) => {
      console.log('Navigation guard: afterNavigate triggered', navigation.to?.url.pathname);
      
      if (this.options.onAfterNavigate) {
        this.options.onAfterNavigate();
      }
    });

    // Setup browser event handlers
    if (browser) {
      this.setupBrowserEventHandlers();
    }
  }

  private setupBrowserEventHandlers() {
    // Handle page unload/refresh
    this.beforeUnloadHandler = (event: BeforeUnloadEvent) => {
      console.log('Navigation guard: beforeunload triggered');
      
      if (this.options.onPageUnload && !this.hasExecutedCleanup) {
        this.options.onPageUnload('page_close');
        this.hasExecutedCleanup = true;
      }

      if (this.options.confirmBeforeLeave) {
        const message = this.options.confirmMessage || 'Are you sure you want to leave?';
        event.preventDefault();
        event.returnValue = message;
        return message;
      }
    };

    // Handle visibility change (tab switching)
    this.visibilityChangeHandler = () => {
      if (this.options.onVisibilityChange) {
        const isVisible = document.visibilityState === 'visible';
        this.options.onVisibilityChange(isVisible);
      }
    };

    window.addEventListener('beforeunload', this.beforeUnloadHandler);
    document.addEventListener('visibilitychange', this.visibilityChangeHandler);
  }

  // Method to manually trigger cleanup
  public async executeCleanup(reason: 'navigation' | 'page_close' | 'manual' = 'manual') {
    if (this.hasExecutedCleanup) return;
    
    console.log(`Navigation guard: Manual cleanup triggered (${reason})`);
    
    if (this.options.onBeforeNavigate) {
      try {
        await this.options.onBeforeNavigate(reason);
        this.hasExecutedCleanup = true;
      } catch (error) {
        console.error('Error in manual cleanup:', error);
      }
    }
  }

  // Method to check if cleanup has been executed
  public hasCleanedUp(): boolean {
    return this.hasExecutedCleanup;
  }

  // Method to reset the cleanup flag (useful for testing or re-initialization)
  public reset() {
    this.hasExecutedCleanup = false;
  }

  // Cleanup method to remove event listeners
  public destroy() {
    if (browser) {
      if (this.beforeUnloadHandler) {
        window.removeEventListener('beforeunload', this.beforeUnloadHandler);
        this.beforeUnloadHandler = null;
      }
      
      if (this.visibilityChangeHandler) {
        document.removeEventListener('visibilitychange', this.visibilityChangeHandler);
        this.visibilityChangeHandler = null;
      }
    }

    // Execute cleanup if it hasn't been done yet
    if (!this.hasExecutedCleanup && this.options.onPageUnload) {
      this.options.onPageUnload('navigation');
      this.hasExecutedCleanup = true;
    }
  }
}

// Convenience function for simple usage
export function useNavigationGuard(options: NavigationGuardOptions): NavigationGuard {
  return new NavigationGuard(options);
}
