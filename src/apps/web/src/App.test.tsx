import { describe, it, expect } from 'vitest';
import { render } from '@testing-library/react';
import App from './App';

describe('App', () => {
  it('should render', () => {
    const { container } = render(<App />);
    expect(container).toBeTruthy();
  });
});