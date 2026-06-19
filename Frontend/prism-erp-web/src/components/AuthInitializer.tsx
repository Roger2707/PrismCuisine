import { useEffect, type ReactNode } from 'react';
import { store } from '../app/store';
import { useAppDispatch, useAppSelector } from '../app/hooks';
import { setAccessToken, setUser, clearAuth, finishHydration } from '../app/userSlice';
import { authApi } from '../services/identityApi';
import type { CurrentUserResponse } from '../services/types/identity.types';

interface AuthInitializerProps {
  children: ReactNode;
}

type HydrateResult = {
  accessToken: string;
  user: CurrentUserResponse;
};

let hydrateSessionPromise: Promise<HydrateResult> | null = null;

function hydrateSessionOnce(): Promise<HydrateResult> {
  if (!hydrateSessionPromise) {
    hydrateSessionPromise = (async () => {
      const tokens = await authApi.refreshPage();
      store.dispatch(setAccessToken(tokens.accessToken));
      const user = await authApi.getCurrentUser();
      return { accessToken: tokens.accessToken, user };
    })().finally(() => {
      hydrateSessionPromise = null;
    });
  }

  return hydrateSessionPromise;
}

export function AuthInitializer({ children }: AuthInitializerProps) {
  const dispatch = useAppDispatch();
  const isHydrating = useAppSelector((state) => state.user.isHydrating);

  useEffect(() => {
    let cancelled = false;

    hydrateSessionOnce()
      .then(({ accessToken, user }) => {
        if (cancelled) return;
        dispatch(setAccessToken(accessToken));
        dispatch(setUser(user));
        dispatch(finishHydration());
      })
      .catch(() => {
        if (!cancelled) {
          dispatch(clearAuth());
        }
      });

    return () => {
      cancelled = true;
    };
  }, [dispatch]);

  if (isHydrating) {
    return (
      <div className="loading" style={{ minHeight: '100vh', display: 'flex', alignItems: 'center', justifyContent: 'center' }}>
        Loading...
      </div>
    );
  }

  return children;
}
