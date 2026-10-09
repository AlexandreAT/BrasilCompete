import { useRouter } from 'expo-router';

import { HOME_EVENT_SECTIONS } from './homeEvents.mock';

export function useAgenda() {
  const router = useRouter();

  function handleNotificationsPress() {
    // As notificações ainda não existem no app.
  }

  function handleSeeAllPress() {
    router.navigate('/calendar');
  }

  return {
    eventSections: HOME_EVENT_SECTIONS,
    handleNotificationsPress,
    handleSeeAllPress,
  };
}
