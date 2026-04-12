import { describe, expectTypeOf, test } from 'vitest';

import type {
  FilePickerAcceptType,
  OpenFilePickerOptions,
  SaveFilePickerOptions,
  WindowOpenFile,
  WindowSaveFile,
} from '@/lib';

describe('File System Types', () => {
  test('should model accepted file type map shape', () => {
    const acceptType: FilePickerAcceptType = {
      description: 'JSON files',
      accept: {
        'application/json': ['.json'],
      },
    };

    expectTypeOf(acceptType.description).toBeString();
    expectTypeOf(acceptType.accept).toEqualTypeOf<Record<string, string[]>>();
  });

  test('should model save picker options shape', () => {
    const options: SaveFilePickerOptions = {
      suggestedName: 'pot-export.json',
      types: [
        {
          description: 'JSON files',
          accept: {
            'application/json': ['.json'],
          },
        },
      ],
    };

    expectTypeOf(options.suggestedName).toBeString();
    expectTypeOf(options.types).toEqualTypeOf<FilePickerAcceptType[]>();
  });

  test('should model open picker options shape', () => {
    const options: OpenFilePickerOptions = {
      types: [
        {
          description: 'JSON files',
          accept: {
            'application/json': ['.json'],
          },
        },
      ],
      multiple: false,
    };

    expectTypeOf(options.types).toEqualTypeOf<FilePickerAcceptType[]>();
    expectTypeOf(options.multiple).toBeBoolean();
  });

  test('should expose window save picker contract', () => {
    expectTypeOf<WindowSaveFile['showSaveFilePicker']>().toEqualTypeOf<
      (options: SaveFilePickerOptions) => Promise<FileSystemFileHandle>
    >();
  });

  test('should expose window open picker contract', () => {
    expectTypeOf<WindowOpenFile['showOpenFilePicker']>().toEqualTypeOf<
      (options: OpenFilePickerOptions) => Promise<FileSystemFileHandle[]>
    >();
  });
});
