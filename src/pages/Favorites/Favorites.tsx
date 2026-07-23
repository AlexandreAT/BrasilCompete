import { Text, View } from 'react-native';

export function Favorites() {
  return (
    <View className="flex-1 items-center justify-center bg-white px-6">
      <Text className="text-3xl font-bold">Favoritos</Text>
      <Text className="mt-2 text-center">
        Seus eventos e participantes favoritos aparecerão aqui.
      </Text>
    </View>
  );
}
