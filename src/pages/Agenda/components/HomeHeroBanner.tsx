import { ImageBackground, StyleSheet, Text, View } from 'react-native';

import { Theme } from '@/shared/themes/Theme';

export function HomeHeroBanner() {
  return (
    <ImageBackground
      source={require('../../../../assets/images/home-banner.webp')}
      accessibilityLabel="Atleta brasileira comemorando em um estádio"
      className="mt-3 overflow-hidden rounded-3xl"
      imageStyle={styles.image}
      resizeMode="cover"
      style={styles.container}
    >
      <View style={styles.overlay} />

      <View className="h-full justify-center pl-4" style={styles.content}>
        <Text
          adjustsFontSizeToFit
          minimumFontScale={0.86}
          numberOfLines={3}
          style={styles.title}
        >
          Acompanhe os{'\n'}próximos eventos{'\n'}do{' '}
          <Text style={styles.countryHighlight}>Brasil</Text>
        </Text>

        <View style={styles.highlightLine} />

        <Text numberOfLines={2} style={styles.description}>
          Datas, horários e onde{'\n'}assistir em um só lugar.
        </Text>
      </View>
    </ImageBackground>
  );
}

const styles = StyleSheet.create({
  container: {
    backgroundColor: Theme.colors.darkNavy,
    height: 198,
  },
  content: {
    width: '68%',
  },
  countryHighlight: {
    color: Theme.colors.primaryGreen,
  },
  description: {
    color: Theme.colors.white,
    fontFamily: Theme.fonts.regular,
    fontSize: 13,
    lineHeight: 18,
  },
  highlightLine: {
    backgroundColor: Theme.colors.highlightYellow,
    borderRadius: 3,
    height: 4,
    marginBottom: 12,
    marginTop: 11,
    width: 48,
  },
  image: {
    borderRadius: 24,
  },
  overlay: {
    ...StyleSheet.absoluteFillObject,
    backgroundColor: 'rgba(3, 25, 55, 0.34)',
  },
  title: {
    color: Theme.colors.white,
    fontFamily: Theme.fonts.bold,
    fontSize: 22,
    letterSpacing: -0.35,
    lineHeight: 27,
  },
});
