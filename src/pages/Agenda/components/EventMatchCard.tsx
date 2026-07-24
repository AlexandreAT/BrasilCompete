import { StyleSheet, Text, View } from 'react-native';

import { Theme } from '@/shared/themes/Theme';

import type { CountryParticipant, HomeEvent } from '../home.types';
import { CountryFlag } from './CountryFlag';

type EventMatchCardProps = {
  event: HomeEvent;
};

type ParticipantProps = {
  participant: CountryParticipant;
};

function Participant({ participant }: ParticipantProps) {
  return (
    <View className="min-w-0 flex-1 items-center">
      <CountryFlag countryName={participant.countryName} size={56} />
      <Text numberOfLines={1} style={styles.countryName}>
        {participant.countryName.toLocaleUpperCase('pt-BR')}
      </Text>
    </View>
  );
}

export function EventMatchCard({ event }: EventMatchCardProps) {
  return (
    <View
      accessible
      accessibilityLabel={`${event.sport}, ${event.stage}. ${event.homeParticipant.countryName} contra ${event.awayParticipant.countryName}, às ${event.time}, no ${event.venue}, ${event.location}.`}
      className="flex-row items-center rounded-3xl bg-white px-3 py-4"
      style={styles.card}
    >
      <Participant participant={event.homeParticipant} />

      <View style={styles.divider} />

      <View className="min-w-0 items-center px-2" style={styles.eventDetails}>
        <Text numberOfLines={1} style={styles.stage}>
          {event.sport} · {event.stage}
        </Text>
        <Text style={styles.time}>{event.time}</Text>
        <Text numberOfLines={1} style={styles.venue}>
          {event.venue}
        </Text>
        <Text numberOfLines={1} style={styles.location}>
          {event.location}
        </Text>
      </View>

      <View style={styles.divider} />

      <Participant participant={event.awayParticipant} />
    </View>
  );
}

const styles = StyleSheet.create({
  card: {
    elevation: 4,
    minHeight: 152,
    shadowColor: Theme.colors.darkNavy,
    shadowOffset: { width: 0, height: 5 },
    shadowOpacity: 0.08,
    shadowRadius: 10,
  },
  countryName: {
    color: Theme.colors.primaryText,
    fontFamily: Theme.fonts.bold,
    fontSize: 11,
    marginTop: 8,
    maxWidth: '100%',
  },
  divider: {
    backgroundColor: Theme.colors.softBorder,
    height: 92,
    width: 1,
  },
  eventDetails: {
    flex: 1.45,
  },
  location: {
    color: Theme.colors.secondaryText,
    fontFamily: Theme.fonts.regular,
    fontSize: 11,
    marginTop: 2,
  },
  stage: {
    color: Theme.colors.primaryText,
    fontFamily: Theme.fonts.medium,
    fontSize: 10,
    maxWidth: '100%',
    textTransform: 'uppercase',
  },
  time: {
    color: Theme.colors.darkNavy,
    fontFamily: Theme.fonts.bold,
    fontSize: 27,
    letterSpacing: -0.6,
    lineHeight: 34,
    marginVertical: 3,
  },
  venue: {
    color: Theme.colors.primaryText,
    fontFamily: Theme.fonts.regular,
    fontSize: 12,
    maxWidth: '100%',
  },
});
