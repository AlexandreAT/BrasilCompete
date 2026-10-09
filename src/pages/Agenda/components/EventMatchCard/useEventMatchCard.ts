import type { EventMatchCardProps } from './types';

export function useEventMatchCard({ event }: EventMatchCardProps) {
  const { awayParticipant, homeParticipant, location, sport, stage, time, venue } =
    event;

  const accessibilityLabel = `${sport}, ${stage}. ${homeParticipant.countryName} contra ${awayParticipant.countryName}, às ${time}, no ${venue}, ${location}.`;
  const competitionLabel = `${sport} · ${stage}`;

  return { accessibilityLabel, competitionLabel };
}
