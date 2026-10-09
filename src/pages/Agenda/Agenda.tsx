import { ChevronRight } from 'lucide-react-native';
import { Fragment } from 'react';

import { DaySectionHeader } from './components/DaySectionHeader/DaySectionHeader';
import { EventMatchCard } from './components/EventMatchCard/EventMatchCard';
import { HomeHeader } from './components/HomeHeader/HomeHeader';
import { HomeHeroBanner } from './components/HomeHeroBanner/HomeHeroBanner';
import {
  Content,
  SafeArea,
  ScreenTitle,
  SeeAllButton,
  SeeAllText,
  seeAllIcon,
  TitleRow,
} from './style';
import { useAgenda } from './useAgenda';

export function Agenda() {
  const { eventSections, handleNotificationsPress, handleSeeAllPress } = useAgenda();

  return (
    <SafeArea>
      <Content>
        <HomeHeader onNotificationsPress={handleNotificationsPress} />
        <HomeHeroBanner />

        <TitleRow>
          <ScreenTitle>Agenda do Brasil</ScreenTitle>

          <SeeAllButton
            accessibilityHint="Abre o calendário completo"
            accessibilityLabel="Ver todos os eventos"
            accessibilityRole="button"
            onPress={handleSeeAllPress}
          >
            <SeeAllText>Ver todos</SeeAllText>
            <ChevronRight {...seeAllIcon} />
          </SeeAllButton>
        </TitleRow>

        {eventSections.map((section) => (
          <Fragment key={section.id}>
            <DaySectionHeader title={section.title} />
            {section.events.map((event) => (
              <EventMatchCard event={event} key={event.id} />
            ))}
          </Fragment>
        ))}
      </Content>
    </SafeArea>
  );
}
