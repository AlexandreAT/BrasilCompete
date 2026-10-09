import { Bell } from 'lucide-react-native';

import {
  Container,
  Logo,
  NotificationButton,
  NotificationDot,
  notificationIcon,
} from './style';
import { type HomeHeaderProps, LOGO_IMAGE } from './types';

export function HomeHeader({ onNotificationsPress }: HomeHeaderProps) {
  return (
    <Container>
      <Logo accessibilityLabel="Logo Brasil Compete" source={LOGO_IMAGE} />

      <NotificationButton
        accessibilityHint="Abre as notificações"
        accessibilityLabel="Notificações"
        accessibilityRole="button"
        onPress={onNotificationsPress}
      >
        <Bell {...notificationIcon} />
        <NotificationDot />
      </NotificationButton>
    </Container>
  );
}
