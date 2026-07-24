import { CalendarDays } from 'lucide-react-native';
import { StyleSheet, Text, View } from 'react-native';

import { Theme } from '@/shared/themes/Theme';

type DaySectionHeaderProps = {
  title: string;
};

export function DaySectionHeader({ title }: DaySectionHeaderProps) {
  return (
    <View className="mb-3 mt-4 flex-row items-center gap-3">
      <View className="items-center justify-center rounded-full bg-white" style={styles.icon}>
        <CalendarDays color={Theme.colors.primaryGreen} size={21} strokeWidth={2} />
      </View>
      <Text style={styles.title}>{title}</Text>
    </View>
  );
}

const styles = StyleSheet.create({
  icon: {
    elevation: 3,
    height: 38,
    shadowColor: Theme.colors.darkNavy,
    shadowOffset: { width: 0, height: 2 },
    shadowOpacity: 0.08,
    shadowRadius: 5,
    width: 38,
  },
  title: {
    color: Theme.colors.primaryText,
    fontFamily: Theme.fonts.bold,
    fontSize: 21,
  },
});
