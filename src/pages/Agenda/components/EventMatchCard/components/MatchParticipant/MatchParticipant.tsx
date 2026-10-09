import { CountryFlag } from '@/pages/Agenda/components/CountryFlag/CountryFlag';

import { Container, CountryName, PARTICIPANT_FLAG_SIZE } from './style';
import type { MatchParticipantProps } from './types';

export function MatchParticipant({ participant }: MatchParticipantProps) {
  return (
    <Container>
      <CountryFlag countryName={participant.countryName} size={PARTICIPANT_FLAG_SIZE} />
      <CountryName numberOfLines={1}>{participant.countryName}</CountryName>
    </Container>
  );
}
