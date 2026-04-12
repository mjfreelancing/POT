import { describe, expect, test } from 'vitest';

import { calculateOptimalTickCount } from '@/lib';

describe('chartUtils', () => {
  describe('calculateOptimalTickCount', () => {
    test('should return minimum of 3 ticks on very small screens', () => {
      const result = calculateOptimalTickCount(320, 20);

      expect(result).toBe(3);
    });

    test('should clamp to data length when data length is lower than base tick count', () => {
      const result = calculateOptimalTickCount(1200, 4);

      expect(result).toBe(4);
    });

    test('should cap tick count at 12 on very large screens', () => {
      const result = calculateOptimalTickCount(4000, 100);

      expect(result).toBe(12);
    });

    test('should follow base calculation when within bounds', () => {
      const result = calculateOptimalTickCount(900, 20);

      expect(result).toBe(6);
    });

    test('should still return at least 3 when data length is below 3', () => {
      const result = calculateOptimalTickCount(900, 2);

      expect(result).toBe(3);
    });

    test('should return 12 when base count exceeds 12 but data allows more', () => {
      const result = calculateOptimalTickCount(2400, 30);

      expect(result).toBe(12);
    });
  });
});
