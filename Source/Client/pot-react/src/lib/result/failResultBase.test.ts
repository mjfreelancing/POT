import { faker } from '@faker-js/faker';

import { FailResultBase } from './failResultBase';

class TestFailResult extends FailResultBase {
  constructor(type: string, code: string, description: string) {
    super(type, code, description);
  }
}

describe('FailResultBase', () => {
  it('should have correct properties with random data', () => {
    const type = faker.word.sample();
    const code = faker.string.alphanumeric(8);
    const description = faker.lorem.sentence();

    const result = new TestFailResult(type, code, description);

    expect(result.type).toBe(type);
    expect(result.code).toBe(code);
    expect(result.description).toBe(description);
  });
});
