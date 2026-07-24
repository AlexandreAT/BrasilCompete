import { useRouter } from 'expo-router';
import { ChevronRight } from 'lucide-react-native';
import { Pressable, ScrollView, StyleSheet, Text, View } from 'react-native';
import { SafeAreaView } from 'react-native-safe-area-context';

import { Theme } from '@/shared/themes/Theme';

import { DaySectionHeader } from './components/DaySectionHeader';
import { EventMatchCard } from './components/EventMatchCard';
import { HomeHeader } from './components/HomeHeader';
import { HomeHeroBanner } from './components/HomeHeroBanner';
import { HOME_EVENT_SECTIONS } from './homeEvents.mock';

export function Agenda() {
  const router = useRouter();

  return (
    <SafeAreaView edges={['top', 'left', 'right']} style={styles.safeArea}>
      <ScrollView
        contentContainerStyle={styles.content}
        showsVerticalScrollIndicator={false}
      >
        <HomeHeader onNotificationsPress={() => undefined} />
        <HomeHeroBanner />

        <View className="mt-6 flex-row items-center justify-between">
          <Text style={styles.screenTitle}>Agenda do Brasil</Text>

          <Pressable
            accessibilityHint="Abre o calendário completo"
            accessibilityLabel="Ver todos os eventos"
            accessibilityRole="button"
            className="flex-row items-center gap-1 py-2 pl-3"
            onPress={() => router.navigate('/calendar')}
            style={({ pressed }) => pressed && styles.pressed}
          >
            <Text style={styles.seeAll}>Ver todos</Text>
            <ChevronRight color={Theme.colors.primaryNavy} size={21} strokeWidth={2.4} />
          </Pressable>
        </View>

        {HOME_EVENT_SECTIONS.map((section) => (
          <View key={section.id}>
            <DaySectionHeader title={section.title} />
            {section.events.map((event) => (
              <EventMatchCard event={event} key={event.id} />
            ))}
          </View>
        ))}
      </ScrollView>
    </SafeAreaView>
  );
}

const styles = StyleSheet.create({
  content: {
    paddingBottom: 24,
    paddingHorizontal: 20,
  },
  pressed: {
    opacity: 0.65,
  },
  safeArea: {
    backgroundColor: Theme.colors.background,
    flex: 1,
  },
  screenTitle: {
    color: Theme.colors.primaryText,
    flexShrink: 1,
    fontFamily: Theme.fonts.bold,
    fontSize: 24,
    letterSpacing: -0.6,
  },
  seeAll: {
    color: Theme.colors.primaryGreen,
    fontFamily: Theme.fonts.semiBold,
    fontSize: 14,
  },
});
