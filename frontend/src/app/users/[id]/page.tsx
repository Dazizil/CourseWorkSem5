import { Suspense } from "react";
import { UserProfile } from "./UserProfile";

// useParams() — runtime-данные; с cacheComponents их нужно оборачивать в Suspense.
export default function UserPage() {
  return (
    <Suspense fallback={<p className="text-zinc-500">Загрузка…</p>}>
      <UserProfile />
    </Suspense>
  );
}
