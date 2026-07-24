import { Bell } from 'lucide-react-native';
import { Image, Pressable, StyleSheet, View } from 'react-native';

import { Theme } from '@/shared/themes/Theme';

type HomeHeaderProps = {
  onNotificationsPress: () => void;
};

export function HomeHeader({ onNotificationsPress }: HomeHeaderProps) {
  return (
    <View className="flex-row items-center justify-between">
      <Image
        source={require('../../../../assets/brand/brasil-compete-logo-transparent.png')}
        accessibilityLabel="Logo Brasil Compete"
        resizeMode="contain"
        style={styles.logo}
      />

      <Pressable
        accessibilityHint="Abre as notificações"
        accessibilityLabel="Notificações"
        accessibilityRole="button"
        className="items-center justify-center rounded-full bg-white"
        onPress={onNotificationsPress}
        style={({ pressed }) => [styles.notificationButton, pressed && styles.pressed]}
      >
        <Bell color={Theme.colors.primaryNavy} size={25} strokeWidth={1.9} />
        <View style={styles.notificationDot} />
      </Pressable>
    </View>
  );
}

const styles = StyleSheet.create({
  logo: {
    height: 68,
    width: 154,
  },
  notificationButton: {
    elevation: 5,
    height: 46,
    shadowColor: Theme.colors.darkNavy,
    shadowOffset: { width: 0, height: 4 },
    shadowOpacity: 0.12,
    shadowRadius: 8,
    width: 46,
  },
  notificationDot: {
    backgroundColor: Theme.colors.primaryGreen,
    borderColor: Theme.colors.white,
    borderRadius: 7,
    borderWidth: 2,
    height: 12,
    position: 'absolute',
    right: 2,
    top: 2,
    width: 12,
  },
  pressed: {
    opacity: 0.75,
    transform: [{ scale: 0.97 }],
  },
});
