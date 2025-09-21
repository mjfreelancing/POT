import { Card } from '@/components/ui/card';
import { PageHeader } from '@/components/layout';

function ProfilePage() {
  return (
    <div className="flex flex-col h-full">
      <PageHeader
        title="Profile"
        subtitle="Manage your profile and preferences"
      />
      <div className="flex-1 flex flex-col items-center justify-center p-6">
        <Card className="w-full max-w-xl p-8">
          <h2 className="text-lg font-semibold mb-4">
            Profile management coming soon.
          </h2>
          {/* Future: Add tabs/sections for Change Password, Preferences, etc. */}
        </Card>
      </div>
    </div>
  );
}

export default ProfilePage;
