import { MatchParticipant } from './components/MatchParticipant/MatchParticipant';
import {
  Card,
  Competition,
  Details,
  Divider,
  Location,
  Time,
  Venue,
} from './style';
import type { EventMatchCardProps } from './types';
import { useEventMatchCard } from './useEventMatchCard';

export function EventMatchCard({ event }: EventMatchCardProps) {
  const { accessibilityLabel, competitionLabel } = useEventMatchCard({ event });

  return (
    <Card accessible accessibilityLabel={accessibilityLabel}>
      <MatchParticipant participant={event.homeParticipant} />

      <Divider />

      <Details>
        <Competition numberOfLines={1}>{competitionLabel}</Competition>
        <Time>{event.time}</Time>
        <Venue numberOfLines={1}>{event.venue}</Venue>
        <Location numberOfLines={1}>{event.location}</Location>
      </Details>

      <Divider />

      <MatchParticipant participant={event.awayParticipant} />
    </Card>
  );
}
