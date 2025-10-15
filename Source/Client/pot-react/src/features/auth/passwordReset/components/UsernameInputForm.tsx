import { useState } from 'react';

import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';

type UsernameInputFormProps = {
  onSubmit: (username: string) => Promise<void>;
  isLoading?: boolean;
};

function UsernameInputForm({
  onSubmit,
  isLoading = false,
}: UsernameInputFormProps) {
  const [username, setUsername] = useState('');

  const handleSubmit = async (e: React.FormEvent<HTMLFormElement>) => {
    e.preventDefault();

    const trimmedUsername = username.trim();

    if (trimmedUsername) {
      await onSubmit(trimmedUsername);
    }
  };

  return (
    <form onSubmit={handleSubmit} className="space-y-6">
      <div className="space-y-3">
        <Label htmlFor="reset-username">Username</Label>
        <Input
          id="reset-username"
          name="username"
          type="text"
          placeholder="Enter your username"
          value={username}
          onChange={e => setUsername(e.target.value)}
          required
          disabled={isLoading}
        />
      </div>

      <Button
        type="submit"
        className="w-full"
        disabled={isLoading || username.trim() === ''}
      >
        {isLoading ? 'Sending...' : 'Send Reset Code'}
      </Button>
    </form>
  );
}

export default UsernameInputForm;
