import { Component, Input } from '@angular/core';

@Component({
  selector: 'app-loading-spinner',
  standalone: true,
  template: `
    <div class="spinner-container" [class.overlay]="overlay">
      <div class="spinner" [style.width.px]="size" [style.height.px]="size"></div>
      @if (message) {
        <p class="spinner-message">{{ message }}</p>
      }
    </div>
  `,
  styles: [`
    .spinner-container {
      display: flex;
      flex-direction: column;
      align-items: center;
      justify-content: center;
      gap: 1rem;
      padding: 2rem;
    }

    .spinner-container.overlay {
      position: absolute;
      inset: 0;
      background: rgba(255, 255, 255, 0.8);
      z-index: 100;
    }

    .spinner {
      border: 3px solid var(--border, #ddd);
      border-top-color: var(--primary, #1976d2);
      border-radius: 50%;
      animation: spin 0.8s linear infinite;
    }

    @keyframes spin {
      to {
        transform: rotate(360deg);
      }
    }

    .spinner-message {
      margin: 0;
      color: var(--muted, #666);
      font-size: 0.9rem;
    }
  `]
})
export class LoadingSpinnerComponent {
  @Input() size = 40;
  @Input() message = '';
  @Input() overlay = false;
}
