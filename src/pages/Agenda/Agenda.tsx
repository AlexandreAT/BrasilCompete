import { Text, View } from 'react-native';

export function Agenda() {
  return (
    <View className="flex-1 items-center justify-center bg-white px-6">
      <Text className="text-3xl font-bold">Agenda</Text>
      <Text className="mt-2 text-center">
        Eventos de hoje, amanhã e dos próximos dias aparecerão aqui.
      </Text>
    </View>
  );
}
